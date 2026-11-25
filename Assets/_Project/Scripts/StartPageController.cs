using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPageController : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "Stage01";     // 튜토리얼 1 씬 이름
    public string stageSelectSceneName = "StageSelect";

    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;

    void Start()
    {
        RefreshButtons();
    }

    void RefreshButtons()
    {
        if (loadGameButton != null)
            loadGameButton.gameObject.SetActive(StageProgress.HasSaveData());
    }

    public void OnClickNewGame()
    {
        // 완전 새 게임 시작
        StageProgress.ResetProgress();
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OnClickLoadGame()
    {
        if (!StageProgress.HasSaveData()) return;
        SceneManager.LoadScene(stageSelectSceneName);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}