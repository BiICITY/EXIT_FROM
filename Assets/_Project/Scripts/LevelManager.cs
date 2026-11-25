using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    bool completed = false;
    bool transitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CompleteLevel()
    {
        if (completed || transitioning) return;

        completed = true;
        transitioning = true;

        // 클리어 순간 플레이어 조작 막기
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc) pc.enabled = false;

        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor) interactor.enabled = false;

        if (StageUIController.Instance != null)
        {
            StageUIController.Instance.StartStageTransition(LoadNextByBuildIndex);
        }
        else
        {
            // UI 컨트롤러가 없으면 그냥 바로 로드
            Time.timeScale = 1f;
            LoadNextByBuildIndex();
        }
    }

    void LoadNextByBuildIndex()
    {
        int idx = SceneManager.GetActiveScene().buildIndex;
        int next = idx + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("DEMO COMPLETE!");
        }
    }
}