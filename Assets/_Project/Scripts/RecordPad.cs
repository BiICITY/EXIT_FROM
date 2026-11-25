using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RecordPad : Interactable
{
    private bool playerInside = false;
    private GlowVisual glow;

    void Awake()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;

        glow = GetComponent<GlowVisual>();
        if (glow != null)
        {
            glow.inactiveColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            glow.activeColor = new Color(0.60f, 1.00f, 0.60f, 1f);
            glow.activeEmission = glow.activeColor;
            glow.emissionIntensity = 2.4f;
            glow.ApplyState(false);
        }
    }

    void Update()
    {
        // 플레이어가 위에 있고, 녹화 중이면 밝게
        if (glow != null)
        {
            bool on = playerInside && GhostSystem.Instance != null && GhostSystem.Instance.IsRecording;
            glow.ApplyState(on);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            playerInside = false;
    }

    public override void Interact(GameObject interactor)
    {
        // 고스트는 못 누르게(플레이어만)
        if (interactor.GetComponent<PlayerController>() == null) return;
        if (!playerInside) return;

        GhostSystem.Instance?.StartRecordingFromPad(transform);
    }
}
