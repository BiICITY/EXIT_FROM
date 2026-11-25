using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColorFloor : MonoBehaviour
{
    public WorldColor floorColor = WorldColor.Blue;

    private Collider col;
    private GlowVisual glow;

    void Awake()
    {
        col = GetComponent<Collider>();
        glow = GetComponent<GlowVisual>();

        // 색 팔레트 자동 세팅
        if (glow != null)
        {
            if (floorColor == WorldColor.Blue)
            {
                glow.inactiveColor = new Color(0.08f, 0.14f, 0.22f, 1f);
                glow.activeColor = new Color(0.25f, 0.70f, 1.00f, 1f);
                glow.activeEmission = glow.activeColor;
            }
            else
            {
                glow.inactiveColor = new Color(0.22f, 0.08f, 0.08f, 1f);
                glow.activeColor = new Color(1.00f, 0.28f, 0.28f, 1f);
                glow.activeEmission = glow.activeColor;
            }
        }
    }

    public void SetActiveCollision(bool active)
    {
        if (col != null) col.enabled = active;
        if (glow != null) glow.ApplyState(active);
    }
}
