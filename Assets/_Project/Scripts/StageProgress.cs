using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageProgress
{
    public const string KEY_UNLOCKED = "GhostRun_UnlockedStage";
    public const string KEY_CLEARED = "GhostRun_ClearedStage";
    public const string KEY_HAS_SAVE = "GhostRun_HasSave";

    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    }

    public static void TouchSave()
    {
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
    }

    public static int GetHighestUnlockedStage()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(KEY_UNLOCKED, 1));
    }

    public static int GetHighestClearedStage()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(KEY_CLEARED, 0));
    }

    public static void UnlockStage(int stageNum)
    {
        int cur = GetHighestUnlockedStage();
        if (stageNum > cur)
        {
            PlayerPrefs.SetInt(KEY_UNLOCKED, stageNum);
            PlayerPrefs.Save();
        }
    }

    public static void MarkCleared(int stageNum = -1, int firstStageBuildIndex = 2)
    {
        if (stageNum <= 0)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            stageNum = (buildIndex - firstStageBuildIndex) + 1;
        }

        int cleared = GetHighestClearedStage();
        if (stageNum > cleared)
        {
            PlayerPrefs.SetInt(KEY_CLEARED, stageNum);
        }

        UnlockStage(stageNum + 1);

        // 저장 데이터 존재 표시
        TouchSave();

        PlayerPrefs.Save();
        Debug.Log($"[StageProgress] Cleared={stageNum}, Unlocked={GetHighestUnlockedStage()}");
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_UNLOCKED);
        PlayerPrefs.DeleteKey(KEY_CLEARED);
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.Save();

        Debug.Log("[StageProgress] ResetProgress done");
    }
}