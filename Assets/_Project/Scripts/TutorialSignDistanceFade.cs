using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialSignDistanceFade : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("비워두면 Tag로 자동 찾음")]
    public Transform player;

    public string playerTag = "Player";

    [Header("Text")]
    [Tooltip("비워두면 자식에서 TMP 텍스트 자동으로 찾음(3D/UGUI 둘 다 가능)")]
    public TMP_Text textAny;

    [Header("Distance")]
    [Tooltip("이 거리 안으로 들어오면 나타남")]
    public float showDistance = 6f;

    [Tooltip("이 거리 밖으로 나가면 사라짐(깜빡임 방지용. showDistance보다 크게 추천)")]
    public float hideDistance = 7f;

    [Header("Fade")]
    public float fadeInTime = 0.35f;
    public float fadeOutTime = 0.35f;
    [Range(0f, 1f)] public float hiddenAlpha = 0f;
    [Range(0f, 1f)] public float shownAlpha = 1f;

    [Header("Debug")]
    public bool debugLogs = false;

    bool visible;
    Coroutine co;

    void Awake()
    {
        if (textAny == null)
            textAny = GetComponentInChildren<TMP_Text>(true);

        SetAlpha(hiddenAlpha);
        visible = false;
    }

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.transform;
        }

        if (debugLogs)
            Debug.Log($"[TutorialSignDistanceFade] player={(player ? player.name : "NULL")} text={(textAny ? textAny.name : "NULL")}");
    }

    void Update()
    {
        // 플레이어가 씬에서 새로 생기거나 교체될 수 있어서 null이면 다시 찾음
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.transform;
            else return;
        }

        if (textAny == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (!visible && dist <= showDistance)
        {
            visible = true;
            FadeTo(shownAlpha, fadeInTime);
            if (debugLogs) Debug.Log($"[TutorialSignDistanceFade] SHOW ({name}) dist={dist:F2}");
        }
        else if (visible && dist >= hideDistance)
        {
            visible = false;
            FadeTo(hiddenAlpha, fadeOutTime);
            if (debugLogs) Debug.Log($"[TutorialSignDistanceFade] HIDE ({name}) dist={dist:F2}");
        }
    }

    void FadeTo(float targetAlpha, float duration)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoFade(targetAlpha, duration));
    }

    System.Collections.IEnumerator CoFade(float targetAlpha, float duration)
    {
        Color c = textAny.color;
        float start = c.a;

        if (duration <= 0f)
        {
            c.a = targetAlpha;
            textAny.color = c;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 일시정지에도 부드럽게
            float a = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(start, targetAlpha, a);
            textAny.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        textAny.color = c;
        co = null;
    }

    void SetAlpha(float a)
    {
        if (textAny == null) return;
        Color c = textAny.color;
        c.a = a;
        textAny.color = c;
    }
}