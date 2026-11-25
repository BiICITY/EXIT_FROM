using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SFX Library")]
public class SFXLibrary : ScriptableObject
{
    [Header("Jump")]
    public AudioClip jump;
    [Range(0f, 1f)] public float jumpVolume = 0.8f;

    [Header("Death")]
    public AudioClip deathLaser;
    [Range(0f, 1f)] public float deathLaserVolume = 1.0f;

    public AudioClip deathFall;
    [Range(0f, 1f)] public float deathFallVolume = 1.0f;

    [Header("Interact")]
    public AudioClip interactBoxStart;
    [Range(0f, 1f)] public float interactBoxStartVolume = 0.9f;

    public AudioClip interactBoxEnd;
    [Range(0f, 1f)] public float interactBoxEndVolume = 0.8f;

    public AudioClip interactColorCube;
    [Range(0f, 1f)] public float interactColorCubeVolume = 0.9f;

    [Header("Box Move Loop (while moving)")]
    public AudioClip boxMoveLoop;
    [Range(0f, 1f)] public float boxMoveLoopVolume = 0.35f;

    [Header("Ghost")]
    public AudioClip recordStart;
    [Range(0f, 1f)] public float recordStartVolume = 0.9f;

    public AudioClip ghostSpawn;
    [Range(0f, 1f)] public float ghostSpawnVolume = 1.0f;

    [Header("UI")]
    public AudioClip uiClick;
    [Range(0f, 1f)] public float uiClickVolume = 0.8f;

    public AudioClip uiConfirm;
    [Range(0f, 1f)] public float uiConfirmVolume = 0.9f;

    public AudioClip uiPauseOpen;
    [Range(0f, 1f)] public float uiPauseOpenVolume = 0.9f;

    public AudioClip uiPauseAction;
    [Range(0f, 1f)] public float uiPauseActionVolume = 0.9f;

    // ✅ 여기부터 BGM 구조 변경
    [Header("BGM - Menu (Start + Stage Select)")]
    public AudioClip bgmMenu;
    [Range(0f, 1f)] public float bgmMenuVolume = 0.35f;

    [Header("BGM - Per Stage (Stage1 = index 0, Stage2 = index 1, ...)")]
    public AudioClip[] stageBgms;
    [Range(0f, 1f)] public float stageBgmVolume = 0.35f;

    [Header("BGM - Fallback (if stageBgms is missing)")]
    public AudioClip bgmGameplayFallback;
    [Range(0f, 1f)] public float bgmGameplayFallbackVolume = 0.35f;

    [Header("Screen Transition")]
    public AudioClip screenFadeOut;
    [Range(0f, 1f)] public float screenFadeOutVolume = 1f;

    public AudioClip screenFadeIn;
    [Range(0f, 1f)] public float screenFadeInVolume = 1f;
}
