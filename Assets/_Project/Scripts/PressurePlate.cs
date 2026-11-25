using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    public bool latch = false; // true면 한 번 밟으면 계속 ON
    public bool IsActive { get; private set; }

    [Header("Activators")]
    [Tooltip("플레이어/고스트(보통 CharacterController)로 활성화")]
    public bool acceptCharacterController = true;

    [Header("Robustness")]
    [Tooltip("트리거 안에 있었던 오브젝트가 파괴/비활성 되어 Exit이 안 올 때, 이 시간 지나면 자동 해제")]
    public float staleSeconds = 0.15f;

    public event Action<bool> StateChanged;

    private bool latched = false;

    // collider -> 마지막으로 감지된 시간
    private readonly Dictionary<Collider, float> lastSeen = new Dictionary<Collider, float>(16);
    private readonly List<Collider> removeBuffer = new List<Collider>(16);

    private GlowVisual glow;

    void Awake()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;

        glow = GetComponent<GlowVisual>();
        if (glow != null)
        {
            glow.inactiveColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            glow.activeColor = new Color(1.00f, 0.95f, 0.35f, 1f);
            glow.activeEmission = glow.activeColor;
            glow.emissionIntensity = 2.8f;
            glow.ApplyState(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsActivator(other)) return;

        lastSeen[other] = Time.time;

        if (latch && !latched)
        {
            latched = true;
            SetActive(true);
        }
        else if (!latch)
        {
            SetActive(true);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsActivator(other)) return;

        lastSeen[other] = Time.time;

        if (!latch)
            SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsActivator(other)) return;

        lastSeen.Remove(other);

        if (!latch)
            RefreshState();
    }

    void FixedUpdate()
    {
        if (latch) return;

        if (staleSeconds > 0f)
            CleanupStale();

        // 상태가 바뀔 가능성이 있을 때만 갱신
        RefreshState();
    }

    private void CleanupStale()
    {
        removeBuffer.Clear();

        foreach (var kv in lastSeen)
        {
            Collider col = kv.Key;

            bool invalid = (col == null) ||
                           !col.enabled ||
                           (col.gameObject == null) ||
                           (Time.time - kv.Value > staleSeconds);

            if (invalid)
                removeBuffer.Add(col);
        }

        for (int i = 0; i < removeBuffer.Count; i++)
            lastSeen.Remove(removeBuffer[i]);
    }

    private void RefreshState()
    {
        SetActive(lastSeen.Count > 0);
    }

    private bool IsActivator(Collider other)
    {
        if (other == null) return false;

        if (acceptCharacterController)
        {
            if (other.GetComponentInParent<CharacterController>() != null)
                return true;
        }

        return false;
    }

    private void SetActive(bool active)
    {
        if (IsActive == active) return;

        IsActive = active;

        if (glow != null) glow.ApplyState(IsActive);
        StateChanged?.Invoke(IsActive);
    }
}
