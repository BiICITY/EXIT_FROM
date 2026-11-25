using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostChapterVoiceBank : MonoBehaviour
{
    public static GhostChapterVoiceBank I { get; private set; }

    [Header("Stage -> Chapter")]
    [Tooltip("첫 스테이지 빌드 인덱스")]
    public int firstStageBuildIndex = 2;

    [Tooltip("챕터별 스테이지 수. 예: 챕터0=2, 챕터1=2, 챕터2=2, 챕터3=2 이면 2,2,2,2")]
    public int[] stagesPerChapter = new int[] { 2, 2, 2, 2 };

    [Header("Timing")]
    public float spawnVoiceDelay = 0.12f;

    [Header("Volume")]
    public float voiceVolume = 1f;

    [Header("Playback")]
    [Tooltip("체크하면 대사/비명을 2D 사운드로 재생합니다. 안 들리는 문제를 줄이기 위해 기본값은 켜두는 것을 추천합니다.")]
    public bool playVoiceAs2D = true;

    [Header("Chapter 0 Spawn Lines")]
    public AudioClip[] chapter0SpawnVoices;

    [Header("Chapter 1 Spawn Lines")]
    public AudioClip[] chapter1SpawnVoices;

    [Header("Chapter 2 Spawn Lines")]
    public AudioClip[] chapter2SpawnVoices;

    [Header("Chapter 3 Spawn Lines")]
    public AudioClip[] chapter3SpawnVoices;

    [Header("Chapter 0 Despawn Screams")]
    public AudioClip[] chapter0DespawnScreams;

    [Header("Chapter 1 Despawn Screams")]
    public AudioClip[] chapter1DespawnScreams;

    [Header("Chapter 2 Despawn Screams")]
    public AudioClip[] chapter2DespawnScreams;

    [Header("Chapter 3 Despawn Screams")]
    public AudioClip[] chapter3DespawnScreams;

    [Header("Fallback")]
    [Tooltip("현재 챕터에 비명 클립이 없으면 다른 챕터 비명 클립이라도 찾아서 재생합니다.")]
    public bool fallbackDespawnScreamToAnyChapter = true;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetCurrentChapter()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int stageOffset = buildIndex - firstStageBuildIndex;

        if (stageOffset < 0)
            return 0;

        int remain = stageOffset;

        if (stagesPerChapter == null || stagesPerChapter.Length == 0)
            return 0;

        for (int chapter = 0; chapter < stagesPerChapter.Length; chapter++)
        {
            int count = Mathf.Max(1, stagesPerChapter[chapter]);

            if (remain < count)
                return chapter;

            remain -= count;
        }

        return stagesPerChapter.Length - 1;
    }

    public void PlayGhostSpawnVoiceOnly(Vector3 pos)
    {
        StartCoroutine(CoPlaySpawnVoice(pos));
    }

    IEnumerator CoPlaySpawnVoice(Vector3 pos)
    {
        if (spawnVoiceDelay > 0f)
            yield return new WaitForSeconds(spawnVoiceDelay);

        int chapter = GetCurrentChapter();
        AudioClip clip = PickSpawnVoice(chapter);

        PlayVoiceClip(clip, pos);
    }

    public void PlayGhostDespawnVoice(Vector3 pos)
    {
        int stageNumber = GetCurrentStageNumber();

        // 스테이지 2 이전에는 비명 재생 안 함
        if (stageNumber <= 9)
            return;

        int chapter = GetCurrentChapter();
        AudioClip clip = PickDespawnVoice(chapter);

        PlayVoiceClip(clip, pos);
    }

    public int GetCurrentStageNumber()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int stageNumber = buildIndex - firstStageBuildIndex + 1;

        return stageNumber;
    }

    void PlayVoiceClip(AudioClip clip, Vector3 pos)
    {
        if (clip == null)
        {
            Debug.LogWarning("[GhostChapterVoiceBank] 재생할 음성 클립이 없습니다. Inspector에 AudioClip이 들어갔는지 확인하세요.");
            return;
        }

        if (AudioManager.I != null)
        {
            if (playVoiceAs2D)
                AudioManager.I.Play2D(clip, voiceVolume, false);
            else
                AudioManager.I.Play3D(clip, pos, voiceVolume, false);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, pos, voiceVolume);
        }
    }

    AudioClip PickSpawnVoice(int chapter)
    {
        switch (chapter)
        {
            case 0: return PickRandom(chapter0SpawnVoices);
            case 1: return PickRandom(chapter1SpawnVoices);
            case 2: return PickRandom(chapter2SpawnVoices);
            case 3: return PickRandom(chapter3SpawnVoices);
            default: return PickRandom(chapter3SpawnVoices);
        }
    }

    AudioClip PickDespawnVoice(int chapter)
    {
        AudioClip clip = null;

        switch (chapter)
        {
            case 0:
                clip = PickRandom(chapter0DespawnScreams);
                break;

            case 1:
                clip = PickRandom(chapter1DespawnScreams);
                break;

            case 2:
                clip = PickRandom(chapter2DespawnScreams);
                break;

            case 3:
                clip = PickRandom(chapter3DespawnScreams);
                break;

            default:
                clip = PickRandom(chapter3DespawnScreams);
                break;
        }

        if (clip != null)
            return clip;

        if (!fallbackDespawnScreamToAnyChapter)
            return null;

        clip = PickRandom(chapter0DespawnScreams);
        if (clip != null) return clip;

        clip = PickRandom(chapter1DespawnScreams);
        if (clip != null) return clip;

        clip = PickRandom(chapter2DespawnScreams);
        if (clip != null) return clip;

        clip = PickRandom(chapter3DespawnScreams);
        if (clip != null) return clip;

        return null;
    }

    AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        int validCount = 0;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int target = Random.Range(0, validCount);
        int seen = 0;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null)
                continue;

            if (seen == target)
                return clips[i];

            seen++;
        }

        return null;
    }
}