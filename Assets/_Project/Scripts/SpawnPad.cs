using UnityEngine;

public class SpawnPad : Interactable
{
    private GlowVisual glow;

    void Awake()
    {
        glow = GetComponent<GlowVisual>();
        if (glow != null)
        {
            glow.inactiveColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            glow.activeColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            glow.activeEmission = glow.activeColor;
            glow.emissionIntensity = 2.0f;
            glow.ApplyState(false);
        }
    }

    void Update()
    {
        if (glow != null)
        {
            bool ready = GhostSystem.Instance != null && GhostSystem.Instance.HasClip && !GhostSystem.Instance.IsRecording;
            glow.ApplyState(ready);
        }
    }

    public override void Interact(GameObject interactor)
    {
        // 고스트는 못 누르게(플레이어만)
        if (interactor.GetComponent<PlayerController>() == null) return;

        GhostSystem.Instance?.SpawnGhost();
    }
}
