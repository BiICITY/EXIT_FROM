using UnityEngine;

public class SetColorSwitch : Interactable
{
    public WorldColor setTo = WorldColor.Blue;

    private GlowVisual glow;

    void Awake()
    {
        glow = GetComponent<GlowVisual>();

        // 버튼 색 팔레트 자동 세팅
        if (glow != null)
        {
            if (setTo == WorldColor.Blue)
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

    void OnEnable()
    {
        WorldLayerManager.ColorChanged += OnWorldColorChanged;
    }

    void OnDisable()
    {
        WorldLayerManager.ColorChanged -= OnWorldColorChanged;
    }

    void Start()
    {
        // 시작 시점 표시 갱신
        if (WorldLayerManager.Instance != null)
            OnWorldColorChanged(WorldLayerManager.Instance.CurrentColor);
    }

    private void OnWorldColorChanged(WorldColor current)
    {
        if (glow != null)
            glow.ApplyState(current == setTo);
    }

    public override void Interact(GameObject interactor)
    {
        if (WorldLayerManager.Instance != null)
            WorldLayerManager.Instance.SetColor(setTo);
    }
}
