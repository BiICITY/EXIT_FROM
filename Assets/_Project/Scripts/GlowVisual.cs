using UnityEngine;

public class GlowVisual : MonoBehaviour
{
    [Header("Auto")]
    public Renderer[] renderers;

    [Header("Compatibility Colors")]
    [Tooltip("기존 Door, ColorFloor, SetColorSwitch 코드 호환용입니다.")]
    public Color inactiveColor = Color.white;

    [Tooltip("기존 Door, ColorFloor, SetColorSwitch 코드 호환용입니다.")]
    public Color activeColor = Color.white;

    [Header("Compatibility Emission")]
    [Tooltip("기존 코드 호환용입니다. 작동 전 Emission은 기본적으로 꺼집니다.")]
    public Color inactiveEmission = Color.black;

    [Tooltip("기존 코드 호환용입니다. White면 원래 Material 색으로 빛납니다.")]
    public Color activeEmission = Color.white;

    [Header("Inactive")]
    [Tooltip("작동하지 않을 때 원래 Material 색을 얼마나 어둡게 할지")]
    [Range(0f, 1f)]
    public float inactiveBrightness = 0.65f;

    [Header("Active")]
    [Tooltip("작동할 때 원래 Material 색을 얼마나 밝게 할지")]
    [Range(0.5f, 3f)]
    public float activeBrightness = 1.25f;

    [Tooltip("작동할 때 Emission 세기")]
    [Range(0f, 20f)]
    public float emissionIntensity = 6f;

    [Tooltip("White면 원래 Material 색 그대로 빛납니다. 색을 넣으면 그 색이 섞입니다.")]
    public Color emissionTint = Color.white;

    private MaterialPropertyBlock mpb;
    private Color[][] originalColors;

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        mpb = new MaterialPropertyBlock();

        originalColors = new Color[renderers.Length][];

        for (int rIndex = 0; rIndex < renderers.Length; rIndex++)
        {
            Renderer r = renderers[rIndex];

            if (!r)
            {
                originalColors[rIndex] = new Color[0];
                continue;
            }

            Material[] mats = r.sharedMaterials;
            originalColors[rIndex] = new Color[mats.Length];

            for (int mIndex = 0; mIndex < mats.Length; mIndex++)
            {
                Material mat = mats[mIndex];

                if (!mat)
                {
                    originalColors[rIndex][mIndex] = Color.white;
                    continue;
                }

                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

                if (mat.HasProperty("_Color"))
                    originalColors[rIndex][mIndex] = mat.GetColor("_Color");
                else
                    originalColors[rIndex][mIndex] = Color.white;
            }
        }

        ApplyState(false);
    }

    public void ApplyState(bool active)
    {
        for (int rIndex = 0; rIndex < renderers.Length; rIndex++)
        {
            Renderer r = renderers[rIndex];
            if (!r) continue;

            Material[] mats = r.sharedMaterials;

            for (int mIndex = 0; mIndex < mats.Length; mIndex++)
            {
                Color original = originalColors[rIndex][mIndex];

                Color baseColor;
                Color emissionColor;

                if (active)
                {
                    baseColor = original * activeBrightness;
                    baseColor.a = original.a;

                    // 원래 Material 색깔 기준 + activeEmission/emissionTint를 섞어서 빛냄
                    emissionColor = original * activeEmission * emissionTint * emissionIntensity;
                    emissionColor.a = original.a;
                }
                else
                {
                    baseColor = original * inactiveBrightness;
                    baseColor.a = original.a;

                    // 작동 전에는 빛번짐 안 나게 Emission 끔
                    emissionColor = inactiveEmission;
                }

                r.GetPropertyBlock(mpb, mIndex);
                mpb.SetColor("_Color", baseColor);
                mpb.SetColor("_EmissionColor", emissionColor);
                r.SetPropertyBlock(mpb, mIndex);
            }
        }
    }

    public void SetActive(bool active)
    {
        ApplyState(active);
    }
}