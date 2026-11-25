using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StageSelectGridPager : MonoBehaviour
{
    [Header("Build Index")]
    public int firstStageBuildIndex = 2;

    [Header("Grid")]
    public Transform gridRoot;
    public Button stageButtonPrefab;
    public int stagesPerPage = 24;

    [Header("Paging UI")]
    public Button prevButton;
    public Button nextButton;
    public TextMeshProUGUI pageText;

    [Header("Chapter Paging")]
    [Tooltip("각 챕터에 들어갈 스테이지 개수. 예: 4,4,5")]
    public int[] chapterStageCounts;

    [Tooltip("챕터 이름. 비워두면 CHAPTER 1, 2 식으로 표시")]
    public string[] chapterNames;

    public TextMeshProUGUI chapterText;

    [Header("Locked Visual (optional)")]
    public bool showLockText = false;
    public string lockText = "LOCK";

    int currentPage = 0;
    int totalStages;
    int totalPages;

    readonly List<Button> spawned = new();

    bool UseChapterMode()
    {
        return chapterStageCounts != null && chapterStageCounts.Length > 0;
    }

    void Start()
    {
        if (gridRoot == null)
        {
            Debug.LogError("[StageSelectGridPager] gridRoot가 비어있어!");
            enabled = false;
            return;
        }

        if (stageButtonPrefab == null)
        {
            Debug.LogError("[StageSelectGridPager] stageButtonPrefab이 비어있어!");
            enabled = false;
            return;
        }

        int lastBuildIndex = SceneManager.sceneCountInBuildSettings - 1;
        totalStages = Mathf.Max(0, lastBuildIndex - firstStageBuildIndex + 1);

        if (UseChapterMode())
        {
            totalPages = chapterStageCounts.Length;
        }
        else
        {
            totalPages = Mathf.Max(1, Mathf.CeilToInt(totalStages / (float)stagesPerPage));
        }

        if (prevButton != null) prevButton.onClick.AddListener(OnPrev);
        if (nextButton != null) nextButton.onClick.AddListener(OnNext);

        RefreshPage();
    }

    void RefreshPage()
    {
        ClearButtons();

        int highestUnlocked = StageProgress.GetHighestUnlockedStage();
        highestUnlocked = Mathf.Clamp(highestUnlocked, 1, Mathf.Max(1, totalStages));

        int startStage;
        int endStage;

        GetPageRange(out startStage, out endStage);

        for (int s = startStage; s < endStage; s++)
        {
            int stageNum = s + 1;
            int buildIndex = firstStageBuildIndex + s;

            var btn = Instantiate(stageButtonPrefab, gridRoot);
            spawned.Add(btn);

            bool unlocked = stageNum <= highestUnlocked;
            btn.interactable = unlocked;

            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                if (!unlocked && showLockText)
                {
                    tmp.text = lockText;
                }
                else
                {
                    int localIndex = s - startStage + 1;
                    tmp.text = UseChapterMode()
                        ? $"{currentPage + 1}-{localIndex}"
                        : stageNum.ToString();
                }
            }

            int idx = buildIndex;
            if (unlocked)
                btn.onClick.AddListener(() => SceneManager.LoadScene(idx));
        }

        if (pageText != null)
            pageText.text = $"{currentPage + 1} / {totalPages}";

        if (chapterText != null)
        {
            if (UseChapterMode())
            {
                if (chapterNames != null &&
                    currentPage < chapterNames.Length &&
                    !string.IsNullOrWhiteSpace(chapterNames[currentPage]))
                {
                    chapterText.text = chapterNames[currentPage];
                }
                else
                {
                    chapterText.text = $"CHAPTER {currentPage + 1}";
                }
            }
            else
            {
                chapterText.text = "";
            }
        }

        if (prevButton != null) prevButton.interactable = currentPage > 0;
        if (nextButton != null) nextButton.interactable = currentPage < totalPages - 1;
    }

    void GetPageRange(out int startStage, out int endStage)
    {
        if (!UseChapterMode())
        {
            startStage = currentPage * stagesPerPage;
            endStage = Mathf.Min(startStage + stagesPerPage, totalStages);
            return;
        }

        startStage = 0;
        for (int i = 0; i < currentPage; i++)
            startStage += Mathf.Max(0, chapterStageCounts[i]);

        int count = Mathf.Max(0, chapterStageCounts[currentPage]);
        endStage = Mathf.Min(startStage + count, totalStages);
    }

    void ClearButtons()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i].gameObject);

        spawned.Clear();
    }

    void OnPrev()
    {
        if (currentPage <= 0) return;
        currentPage--;
        RefreshPage();
    }

    void OnNext()
    {
        if (currentPage >= totalPages - 1) return;
        currentPage++;
        RefreshPage();
    }
}