using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalZone : MonoBehaviour
{
    private GlowVisual glow;

    void Awake()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;

        glow = GetComponent<GlowVisual>();
        if (glow != null)
        {
            glow.inactiveColor = new Color(0.10f, 0.20f, 0.10f, 1f);
            glow.activeColor = new Color(0.25f, 1.00f, 0.35f, 1f);
            glow.activeEmission = glow.activeColor;
            glow.emissionIntensity = 2.8f;
            glow.ApplyState(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null) return;
        LevelManager.Instance?.CompleteLevel();
    }
}
