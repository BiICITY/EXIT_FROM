using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSystemController : MonoBehaviour
{
    [Header("Scene Index Rules")]
    [Tooltip("Stage01의 Build Index. StartPage(0), StageSelect(1), Stage01(2)면 2")]
    public int firstStageBuildIndex = 2;

    [Tooltip("StageSelect 씬 Build Index (보통 1)")]
    public int stageSelectBuildIndex = 1;

    [Header("UI References")]
    public Button pauseButton;
    public CanvasGroup pauseMenuGroup;
    public Button continueButton;
    public Button restartButton;
    public Button exitButton;

    [Header("Options")]
    public bool toggleWithEsc = true;
    public bool pauseAudio = true;
    public bool manageCursor = true;

    [Tooltip("일시정지 동안 꺼둘 스크립트(플레이어 이동/카메라 등)")]
    public MonoBehaviour[] disableWhilePaused;

    bool paused;
    float prevTimeScale = 1f;

    void Awake()
    {
        // 싱글톤(중복 방지)
        var existing = FindObjectsByType<PauseSystemController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // 버튼 이벤트 연결
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (continueButton != null) continueButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(RestartStage);
        if (exitButton != null) exitButton.onClick.AddListener(ExitToStageSelect);

        HideMenuInstant();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyVisibilityByScene();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (toggleWithEsc && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀌면 안전하게 정상 복귀
        ForceResume();
        ApplyVisibilityByScene();
    }

    bool IsStageScene()
    {
        return SceneManager.GetActiveScene().buildIndex >= firstStageBuildIndex;
    }

    void ApplyVisibilityByScene()
    {
        // 메뉴 씬(StartPage, StageSelect)에서는 일시정지 버튼 숨김
        bool stage = IsStageScene();

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(stage);

        if (!stage)
            HideMenuInstant();
    }

    public void TogglePause()
    {
        // 스테이지 씬에서만 동작
        if (!IsStageScene()) return;

        if (paused) Resume();
        else Pause();
    }

    void Pause()
    {
        paused = true;
        prevTimeScale = Time.timeScale;

        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        SetMenuVisible(true);
        SetControlEnabled(false);

        if (manageCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Resume()
    {
        paused = false;

        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
        if (pauseAudio) AudioListener.pause = false;

        SetMenuVisible(false);
        SetControlEnabled(true);

        if (manageCursor)
        {
            // 너 게임이 마우스 락이 필요 없으면 None으로 바꿔도 됨
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void ForceResume()
    {
        paused = false;
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        SetMenuVisible(false);
        SetControlEnabled(true);

        if (manageCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void RestartStage()
    {
        ForceResume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitToStageSelect()
    {
        ForceResume();
        SceneManager.LoadScene(stageSelectBuildIndex);
    }

    void SetMenuVisible(bool visible)
    {
        if (pauseMenuGroup == null) return;

        pauseMenuGroup.alpha = visible ? 1f : 0f;
        pauseMenuGroup.interactable = visible;
        pauseMenuGroup.blocksRaycasts = visible;
    }

    void HideMenuInstant()
    {
        if (pauseMenuGroup == null) return;

        pauseMenuGroup.alpha = 0f;
        pauseMenuGroup.interactable = false;
        pauseMenuGroup.blocksRaycasts = false;
    }

    void SetControlEnabled(bool enabled)
    {
        if (disableWhilePaused == null) return;
        foreach (var mb in disableWhilePaused)
        {
            if (mb != null) mb.enabled = enabled;
        }
    }
}
