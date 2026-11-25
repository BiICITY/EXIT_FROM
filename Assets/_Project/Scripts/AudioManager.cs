using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Library")]
    public SFXLibrary sfx;

    [Header("2D SFX Source (auto created if empty)")]
    public AudioSource sfx2D;

    [Header("3D SFX Prefab (optional)")]
    public AudioSource sfx3DPrefab;

    [Header("BGM Source (auto created if empty)")]
    public AudioSource bgmSource;

    [Header("BGM Fade")]
    public float bgmFadeTime = 0.35f;

    [Header("Auto BGM by Scene Build Index")]
    public bool autoBgm = true;

    [Tooltip("스타트 페이지 빌드 인덱스")]
    public int startPageBuildIndex = 0;

    [Tooltip("스테이지 선택 페이지 빌드 인덱스")]
    public int stageSelectBuildIndex = 1;

    [Tooltip("첫 스테이지 빌드 인덱스")]
    public int firstStageBuildIndex = 2;

    [Header("Pitch Randomize (SFX only)")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    Coroutine bgmCo;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (sfx2D == null)
        {
            sfx2D = gameObject.AddComponent<AudioSource>();
            sfx2D.playOnAwake = false;
            sfx2D.loop = false;
            sfx2D.spatialBlend = 0f;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (autoBgm)
            ApplySceneBgm(SceneManager.GetActiveScene().buildIndex);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoBgm)
            ApplySceneBgm(scene.buildIndex);
    }

    float RandPitch()
    {
        return Random.Range(pitchRange.x, pitchRange.y);
    }

    // ---------- SFX ----------
    public void Play2D(AudioClip clip, float volume = 1f, bool randomPitch = true)
    {
        if (clip == null || sfx2D == null)
            return;

        sfx2D.pitch = randomPitch ? RandPitch() : 1f;
        sfx2D.PlayOneShot(clip, volume);
    }

    public void Play3D(AudioClip clip, Vector3 pos, float volume = 1f, bool randomPitch = true)
    {
        if (clip == null)
            return;

        if (sfx3DPrefab == null)
        {
            Play2D(clip, volume, randomPitch);
            return;
        }

        AudioSource src = Instantiate(sfx3DPrefab, pos, Quaternion.identity);
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;
        src.pitch = randomPitch ? RandPitch() : 1f;
        src.PlayOneShot(clip, volume);

        Destroy(src.gameObject, clip.length + 0.2f);
    }

    // ---------- BGM ----------
    void ApplySceneBgm(int buildIndex)
    {
        if (sfx == null)
            return;

        if (buildIndex == startPageBuildIndex || buildIndex == stageSelectBuildIndex)
        {
            PlayBgm(sfx.bgmMenu, sfx.bgmMenuVolume);
            return;
        }

        if (buildIndex >= firstStageBuildIndex)
        {
            int stageIndex = buildIndex - firstStageBuildIndex;
            AudioClip clip = null;
            float vol = sfx.stageBgmVolume;

            if (sfx.stageBgms != null && stageIndex >= 0 && stageIndex < sfx.stageBgms.Length)
                clip = sfx.stageBgms[stageIndex];

            if (clip == null)
            {
                clip = sfx.bgmGameplayFallback;
                vol = sfx.bgmGameplayFallbackVolume;
            }

            PlayBgm(clip, vol);
            return;
        }

        StopBgm();
    }

    public void PlayBgm(AudioClip clip, float volume)
    {
        if (bgmSource == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            bgmSource.volume = volume;
            return;
        }

        if (bgmCo != null)
            StopCoroutine(bgmCo);

        bgmCo = StartCoroutine(CoSwitchBgm(clip, volume));
    }

    public void StopBgm()
    {
        if (bgmSource == null)
            return;

        if (bgmCo != null)
            StopCoroutine(bgmCo);

        bgmCo = StartCoroutine(CoSwitchBgm(null, 0f));
    }

    IEnumerator CoSwitchBgm(AudioClip next, float nextVol)
    {
        float t = 0f;
        float startVol = bgmSource.volume;

        while (t < bgmFadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        bgmSource.clip = next;

        if (next == null)
        {
            bgmCo = null;
            yield break;
        }

        bgmSource.Play();

        t = 0f;
        while (t < bgmFadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, nextVol, t / bgmFadeTime);
            yield return null;
        }

        bgmSource.volume = nextVol;
        bgmCo = null;
    }

    // ---------- High-level SFX helpers ----------
    public void Jump()
    {
        Play2D(sfx?.jump, sfx ? sfx.jumpVolume : 1f, true);
    }

    public void DeathLaser(Vector3 pos)
    {
        Play3D(sfx?.deathLaser, pos, sfx ? sfx.deathLaserVolume : 1f, false);
    }

    public void DeathFall(Vector3 pos)
    {
        Play3D(sfx?.deathFall, pos, sfx ? sfx.deathFallVolume : 1f, false);
    }

    public void InteractBoxStart(Vector3 pos)
    {
        Play3D(sfx?.interactBoxStart, pos, sfx ? sfx.interactBoxStartVolume : 1f, true);
    }

    public void InteractBoxEnd(Vector3 pos)
    {
        Play3D(sfx?.interactBoxEnd, pos, sfx ? sfx.interactBoxEndVolume : 1f, true);
    }

    public void InteractColorCube(Vector3 pos)
    {
        Play3D(sfx?.interactColorCube, pos, sfx ? sfx.interactColorCubeVolume : 1f, true);
    }

    public void RecordStart()
    {
        Play2D(sfx?.recordStart, sfx ? sfx.recordStartVolume : 1f, false);
    }

    public void GhostSpawn(Vector3 pos)
    {
        Play3D(sfx?.ghostSpawn, pos, sfx ? sfx.ghostSpawnVolume : 1f, false);

        if (GhostChapterVoiceBank.I != null)
            GhostChapterVoiceBank.I.PlayGhostSpawnVoiceOnly(pos);
    }

    public void GhostDespawn(Vector3 pos)
    {
        if (GhostChapterVoiceBank.I != null)
            GhostChapterVoiceBank.I.PlayGhostDespawnVoice(pos);
    }

    public void ScreenFadeOut()
    {
        Play2D(sfx?.screenFadeOut, sfx ? sfx.screenFadeOutVolume : 1f, false);
    }

    public void ScreenFadeIn()
    {
        Play2D(sfx?.screenFadeIn, sfx ? sfx.screenFadeInVolume : 1f, false);
    }

    public void UIClick()
    {
        Play2D(sfx?.uiClick, sfx ? sfx.uiClickVolume : 1f, false);
    }

    public void UIConfirm()
    {
        Play2D(sfx?.uiConfirm, sfx ? sfx.uiConfirmVolume : 1f, false);
    }

    public void UIPauseOpen()
    {
        Play2D(sfx?.uiPauseOpen, sfx ? sfx.uiPauseOpenVolume : 1f, false);
    }

    public void UIPauseAction()
    {
        Play2D(sfx?.uiPauseAction, sfx ? sfx.uiPauseActionVolume : 1f, false);
    }
}