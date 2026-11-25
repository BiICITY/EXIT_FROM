using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Plate Rule")]
    [Tooltip("비워두면 씬의 모든 PressurePlate를 조건으로 사용")]
    public string requiredPlateTag = ""; // 예: "StagePlate" (선택)

    [Header("Collision / Laser")]
    [Tooltip("문이 열리는 동안/열려있는 동안, 플레이어가 통과하도록 Collider를 Trigger로 바꿉니다.\n(레이저는 QueryTriggerInteraction.Ignore라서 Trigger면 '막지 않음')")]
    public bool passThroughWhenOpen = true;

    [Tooltip("문이 닫히는 동안엔 끼임 방지를 위해 Trigger 유지 → 완전히 닫힌 뒤 Solid로 복귀")]
    public bool safePassThroughWhileClosing = true;

    [Header("Camera Shake")]
    public bool cameraShakeOnOpen = true;
    public float cameraShakeAmplitude = 0.18f;
    public float cameraShakeDuration = 0.12f;
    public float cameraShakeFrequency = 35f;

    [Header("Move Target")]
    public Vector3 openOffset = new Vector3(0f, 5f, 0f);

    [Header("Move Feel")]
    [Tooltip("값이 클수록 더 빠르게 이동(거리/속도로 duration 계산)")]
    public float moveSpeed = 16f;

    [Tooltip("최소 이동 시간(너무 짧아지는 것 방지)")]
    public float minMoveDuration = 0.10f;

    [Tooltip("열릴 때 목표보다 더 올라가는 높이(쿵 느낌)")]
    public float overshootY = 0.35f;

    [Tooltip("오버슈트 후 제자리로 정착하는 시간")]
    public float settleDuration = 0.10f;

    [Header("Shake")]
    [Tooltip("열릴 때 마지막에 살짝 떠는 시간")]
    public float shakeDuration = 0.08f;

    [Tooltip("떨림 강도")]
    public float shakeAmplitude = 0.05f;

    [Tooltip("떨림 빠르기(Hz 느낌)")]
    public float shakeFrequency = 35f;

    private Vector3 closedPos;
    private Vector3 openPos;

    private bool isOpen;
    private Collider col;
    private GlowVisual glow;

    private readonly List<PressurePlate> plates = new List<PressurePlate>(8);
    private Coroutine animCo;

    void Awake()
    {
        closedPos = transform.position;
        openPos = closedPos + openOffset;

        col = GetComponent<Collider>();
        glow = GetComponent<GlowVisual>();

        // 시작은 "닫힘" 기준: 밟기 가능 + 레이저 막기
        SetSolidCollision(true);

        if (glow != null)
        {
            glow.inactiveColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            glow.activeColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            glow.activeEmission = glow.activeColor;
            glow.emissionIntensity = 2.2f;
            glow.ApplyState(false);
        }
    }

    void OnEnable()
    {
        RefreshPlates();
        SubscribePlates();
        Evaluate();
    }

    void OnDisable()
    {
        UnsubscribePlates();

        if (animCo != null)
        {
            StopCoroutine(animCo);
            animCo = null;
        }
    }

    /// <summary>씬의 모든 PressurePlate를 찾아 조건 목록으로 사용</summary>
    public void RefreshPlates()
    {
        plates.Clear();

#if UNITY_2023_1_OR_NEWER
        var all = FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var all = FindObjectsOfType<PressurePlate>(true);
#endif

        for (int i = 0; i < all.Length; i++)
        {
            var p = all[i];
            if (p == null) continue;

            if (!string.IsNullOrEmpty(requiredPlateTag) && !p.CompareTag(requiredPlateTag))
                continue;

            plates.Add(p);
        }
    }

    private void SubscribePlates()
    {
        for (int i = 0; i < plates.Count; i++)
        {
            var p = plates[i];
            if (p == null) continue;
            p.StateChanged += OnPlateChanged;
        }
    }

    private void UnsubscribePlates()
    {
        for (int i = 0; i < plates.Count; i++)
        {
            var p = plates[i];
            if (p == null) continue;
            p.StateChanged -= OnPlateChanged;
        }
    }

    private void OnPlateChanged(bool _) => Evaluate();

    private void Evaluate()
    {
        if (plates.Count == 0)
        {
            SetOpen(false);
            return;
        }

        for (int i = 0; i < plates.Count; i++)
        {
            var p = plates[i];
            if (p == null || !p.IsActive)
            {
                SetOpen(false);
                return;
            }
        }

        SetOpen(true);
    }

    private void SetOpen(bool open)
    {
        if (isOpen == open) return;
        isOpen = open;

        // Inspector에서 openOffset 바꿨을 때 반영
        openPos = closedPos + openOffset;

        if (glow != null) glow.ApplyState(isOpen);

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimateDoor(isOpen));
    }

    private IEnumerator AnimateDoor(bool open)
    {
        Vector3 start = transform.position;
        Vector3 end = open ? openPos : closedPos;

        float dist = Vector3.Distance(start, end);
        float dur = Mathf.Max(minMoveDuration, dist / Mathf.Max(0.01f, moveSpeed));

        if (open)
        {
            // ✅ 열릴 때: 통과 가능(Trigger)로 → 플레이어도 통과, 레이저도 통과
            if (passThroughWhenOpen) SetSolidCollision(false);

            Vector3 over = end + Vector3.up * Mathf.Max(0f, overshootY);

            // 1) 오버슈트까지
            yield return Move(start, over, dur * 0.75f, EaseOutCubic);

            // 2) 정착
            yield return Move(over, end, Mathf.Max(0.02f, settleDuration), EaseInOutCubic);

            if (cameraShakeOnOpen && CameraShake.Instance != null)
                CameraShake.Instance.Shake(cameraShakeAmplitude, cameraShakeDuration, cameraShakeFrequency);

            // 3) 잔진동
            yield return Shake(end, shakeDuration, shakeAmplitude, shakeFrequency);

            transform.position = end;
        }
        else
        {
            // ✅ 닫힐 때: 이동 중 끼임 방지로 Trigger 유지(옵션) → 닫힌 뒤 Solid로 복귀
            if (safePassThroughWhileClosing) SetSolidCollision(false);

            yield return Move(start, end, dur, EaseInOutCubic);
            transform.position = end;

            // 완전히 닫힌 뒤에만 Solid로 (밟기 가능 + 레이저 막기)
            SetSolidCollision(true);
        }

        animCo = null;
    }

    /// <summary>
    /// solid=true : 콜라이더 solid(밟기 가능, 레이저 막힘)
    /// solid=false: 콜라이더 trigger(통과 가능, 레이저도 통과: LaserBeamHazard가 Trigger를 무시하기 때문)
    /// </summary>
    private void SetSolidCollision(bool solid)
    {
        if (col == null) return;

        col.enabled = true;         // 아예 끄지 않고 항상 켜둠
        col.isTrigger = !solid;     // solid일 때만 밟기/막기 가능
    }

    private IEnumerator Move(Vector3 from, Vector3 to, float duration, System.Func<float, float> ease)
    {
        if (duration <= 0f)
        {
            transform.position = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            float e = ease != null ? ease(a) : a;
            transform.position = Vector3.LerpUnclamped(from, to, e);
            yield return null;
        }
        transform.position = to;
    }

    private IEnumerator Shake(Vector3 basePos, float duration, float amp, float freq)
    {
        if (duration <= 0f || amp <= 0f || freq <= 0f)
            yield break;

        float t = 0f;
        float w = Mathf.PI * 2f * freq;

        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Sin(t * w);
            transform.position = basePos + Vector3.up * (s * amp);
            yield return null;
        }
        transform.position = basePos;
    }

    // Easing
    private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private float EaseInOutCubic(float t) =>
        t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
}
