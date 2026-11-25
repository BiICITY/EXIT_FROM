using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageUIController : MonoBehaviour
{
    public static StageUIController Instance { get; private set; }

    [Header("Stage Index (Build Profiles)")]
    public int firstStageBuildIndex = 2;

    [Header("Stage Banner")]
    public CanvasGroup stageBannerGroup;
    public TextMeshProUGUI stageBannerText;
    public float bannerFadeIn = 0.35f;
    public float bannerHold = 0.9f;
    public float bannerFadeOut = 0.35f;

    [Header("Custom Stage Titles")]
    public string[] customStageTitles;

    [Header("Screen Transition")]
    public CanvasGroup screenFadeGroup;
    public float fadeToBlackTime = 0.35f;
    public float blackHoldTime = 0.1f;
    public float fadeFromBlackTime = 0.35f;

    [Header("Legacy Clear UI (Unused)")]
    public CanvasGroup clearGroup;
    public TextMeshProUGUI clearTitleText;
    public TextMeshProUGUI clearHintText;

    Coroutine bannerRoutine;
    Coroutine transitionRoutine;

    bool delayBannerUntilTransitionEnds = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
        HideClearInstant();
        HideFadeInstant();
        PlayStageBanner();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideClearInstant();

        if (delayBannerUntilTransitionEnds)
            return;

        PlayStageBanner();
    }

    bool IsStageScene()
    {
        return SceneManager.GetActiveScene().buildIndex >= firstStageBuildIndex;
    }

    int GetStageNumber()
    {
        return SceneManager.GetActiveScene().buildIndex - firstStageBuildIndex + 1;
    }

    string GetStageBannerText()
    {
        int stageIndex = GetStageNumber() - 1;

        if (customStageTitles != null &&
            stageIndex >= 0 &&
            stageIndex < customStageTitles.Length &&
            !string.IsNullOrWhiteSpace(customStageTitles[stageIndex]))
        {
            return customStageTitles[stageIndex];
        }

        return $"STAGE {GetStageNumber()}";
    }

    void PlayStageBanner()
    {
        if (!IsStageScene())
        {
            if (stageBannerGroup != null)
                stageBannerGroup.alpha = 0f;
            return;
        }

        if (stageBannerText != null)
            stageBannerText.text = GetStageBannerText();

        if (bannerRoutine != null)
            StopCoroutine(bannerRoutine);

        bannerRoutine = StartCoroutine(CoStageBanner());
    }

    public void StartStageTransition(Action onBlackLoadAction)
    {
        if (transitionRoutine != null) return;
        transitionRoutine = StartCoroutine(CoStageTransition(onBlackLoadAction));
    }

    public void HideClearInstant()
    {
        if (clearGroup != null)
        {
            clearGroup.alpha = 0f;
            clearGroup.blocksRaycasts = false;
            clearGroup.interactable = false;
        }
    }

    void HideFadeInstant()
    {
        if (screenFadeGroup != null)
        {
            screenFadeGroup.alpha = 0f;
            screenFadeGroup.blocksRaycasts = false;
            screenFadeGroup.interactable = false;
        }
    }

    IEnumerator CoStageBanner()
    {
        if (stageBannerGroup == null) yield break;

        stageBannerGroup.alpha = 0f;

        yield return Fade(stageBannerGroup, 0f, 1f, bannerFadeIn);
        yield return new WaitForSecondsRealtime(bannerHold);
        yield return Fade(stageBannerGroup, 1f, 0f, bannerFadeOut);

        stageBannerGroup.alpha = 0f;
        bannerRoutine = null;
    }

    IEnumerator CoStageTransition(Action onBlackLoadAction)
    {
        if (bannerRoutine != null)
        {
            StopCoroutine(bannerRoutine);
            bannerRoutine = null;
        }

        if (stageBannerGroup != null)
            stageBannerGroup.alpha = 0f;

        delayBannerUntilTransitionEnds = true;

        if (screenFadeGroup == null)
        {
            onBlackLoadAction?.Invoke();
            delayBannerUntilTransitionEnds = false;
            PlayStageBanner();
            transitionRoutine = null;
            yield break;
        }

        screenFadeGroup.blocksRaycasts = true;
        screenFadeGroup.interactable = true;

        AudioManager.I?.ScreenFadeOut();
        yield return Fade(screenFadeGroup, 0f, 1f, fadeToBlackTime);

        if (blackHoldTime > 0f)
            yield return new WaitForSecondsRealtime(blackHoldTime);

        onBlackLoadAction?.Invoke();

        yield return null;
        yield return null;

        AudioManager.I?.ScreenFadeIn();
        yield return Fade(screenFadeGroup, 1f, 0f, fadeFromBlackTime);

        screenFadeGroup.alpha = 0f;
        screenFadeGroup.blocksRaycasts = false;
        screenFadeGroup.interactable = false;

        delayBannerUntilTransitionEnds = false;
        PlayStageBanner();

        transitionRoutine = null;
    }

    IEnumerator Fade(CanvasGroup g, float from, float to, float duration)
    {
        if (g == null) yield break;

        if (duration <= 0f)
        {
            g.alpha = to;
            yield break;
        }

        float t = 0f;
        g.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            g.alpha = Mathf.Lerp(from, to, a);
            yield return null;
        }

        g.alpha = to;
    }
}