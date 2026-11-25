using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GhostRecorderSystem : MonoBehaviour
{
    public static GhostRecorderSystem Instance { get; private set; }

    [Header("Keys - Record (3 slots)")]
    public KeyCode recordKey1 = KeyCode.Q;
    public KeyCode recordKey2 = KeyCode.W;
    public KeyCode recordKey3 = KeyCode.E;

    [Header("Keys - Spawn (3 ghosts)")]
    public KeyCode spawnKey1 = KeyCode.A;
    public KeyCode spawnKey2 = KeyCode.S;
    public KeyCode spawnKey3 = KeyCode.D;

    [Header("Keys - In recording")]
    public KeyCode interactKey = KeyCode.F;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Reset")]
    public KeyCode resetStageKey = KeyCode.R;
    public bool clearAllOnSceneLoad = true;

    [Header("Recording")]
    public float maxRecordSeconds = 5f;
    public int maxUsesPerSlot = 2;

    [Header("References")]
    public PlayerController playerController;
    public GameObject ghostPrefab;

    [Header("Recording UI (Optional)")]
    public CanvasGroup recGroup;
    public Image recDot;
    public TextMeshProUGUI recTimerText;
    public float blinkHz = 2f;

    private readonly List<GhostFrame>[] clips = new List<GhostFrame>[3]
    {
        new List<GhostFrame>(),
        new List<GhostFrame>(),
        new List<GhostFrame>()
    };

    private readonly Vector3[] startPos = new Vector3[3];
    private readonly Quaternion[] startRot = new Quaternion[3];
    private readonly int[] usesRemaining = new int[3];
    private readonly GameObject[] activeGhost = new GameObject[3];

    private readonly bool[] recording = new bool[3];
    private readonly float[] recordTimer = new float[3];

    private Vector2 cachedMove;
    private bool interactQueued;
    private bool jumpQueued;

    bool AnyRecording => recording[0] || recording[1] || recording[2];

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
        FindPlayerIfNeeded();
        SetRecUI(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerIfNeeded();

        if (clearAllOnSceneLoad)
            ClearAllSlotsAndGhosts();
        else
            StopAllRecordingOnly();

        SetRecUI(false);
    }

    void FindPlayerIfNeeded()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(resetStageKey))
        {
            ResetStage();
            return;
        }

        if (Input.GetKeyDown(recordKey1)) ToggleRecord(0);
        if (Input.GetKeyDown(recordKey2)) ToggleRecord(1);
        if (Input.GetKeyDown(recordKey3)) ToggleRecord(2);

        if (Input.GetKeyDown(spawnKey1)) SpawnGhost(0);
        if (Input.GetKeyDown(spawnKey2)) SpawnGhost(1);
        if (Input.GetKeyDown(spawnKey3)) SpawnGhost(2);

        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;

        cachedMove = new Vector2(h, v);

        if (cachedMove.sqrMagnitude > 1f)
            cachedMove.Normalize();

        if (Input.GetKeyDown(interactKey))
            interactQueued = true;

        if (Input.GetKeyDown(jumpKey))
            jumpQueued = true;

        SetRecUI(AnyRecording);
    }

    void FixedUpdate()
    {
        if (!AnyRecording)
            return;

        for (int slot = 0; slot < 3; slot++)
        {
            if (!recording[slot])
                continue;

            recordTimer[slot] += Time.fixedDeltaTime;

            clips[slot].Add(new GhostFrame
            {
                move = cachedMove,
                interact = interactQueued,
                jump = jumpQueued
            });

            if (recordTimer[slot] >= maxRecordSeconds)
                StopRecording(slot);
        }

        interactQueued = false;
        jumpQueued = false;
    }

    void ToggleRecord(int slot)
    {
        FindPlayerIfNeeded();

        if (playerController == null)
        {
            Debug.LogWarning("[GhostRecorderSystem] PlayerController가 없습니다.");
            return;
        }

        if (!recording[slot])
            StartRecording(slot);
        else
            StopRecording(slot);
    }

    void StartRecording(int slot)
    {
        clips[slot].Clear();
        recordTimer[slot] = 0f;
        interactQueued = false;
        jumpQueued = false;

        startPos[slot] = playerController.transform.position;
        startRot[slot] = playerController.transform.rotation;

        recording[slot] = true;

        Debug.Log($"RECORD START slot={slot + 1}");
    }

    void StopRecording(int slot)
    {
        if (!recording[slot])
            return;

        recording[slot] = false;

        Debug.Log($"RECORD STOP slot={slot + 1} frames={clips[slot].Count}");

        if (clips[slot].Count > 0)
            usesRemaining[slot] = maxUsesPerSlot;
    }

    void StopAllRecordingOnly()
    {
        for (int i = 0; i < 3; i++)
            recording[i] = false;

        interactQueued = false;
        jumpQueued = false;
    }

    void SpawnGhost(int slot)
    {
        if (AnyRecording)
            return;

        if (ghostPrefab == null)
        {
            Debug.LogWarning("[GhostRecorderSystem] Ghost Prefab이 비어 있습니다. Inspector에서 ghostPrefab을 넣어주세요.");
            return;
        }

        if (clips[slot] == null || clips[slot].Count == 0)
        {
            Debug.LogWarning($"[GhostRecorderSystem] Slot {slot + 1}에 녹화 데이터가 없습니다.");
            return;
        }

        if (usesRemaining[slot] <= 0)
        {
            Debug.Log($"Ghost {slot + 1} has no uses left.");
            return;
        }

        // 기존 고스트 제거
        GameObject oldGhost = activeGhost[slot];

        if (oldGhost != null)
        {
            PlayDespawnVoiceForGhost(oldGhost);
            Destroy(oldGhost);
            activeGhost[slot] = null;
        }

        // 새 고스트 생성
        GameObject go = Instantiate(ghostPrefab, startPos[slot], startRot[slot]);

        if (go == null)
        {
            Debug.LogWarning("[GhostRecorderSystem] 고스트 생성에 실패했습니다.");
            return;
        }

        activeGhost[slot] = go;

        GhostAgent agent = go.GetComponent<GhostAgent>();

        if (agent != null)
        {
            if (playerController != null)
            {
                agent.moveSpeed = playerController.moveSpeed;
                agent.gravity = playerController.gravity;
                agent.jumpHeight = playerController.jumpHeight;
            }

            agent.Init(clips[slot]);
        }
        else
        {
            Debug.LogWarning("[GhostRecorderSystem] 생성된 고스트에 GhostAgent가 없습니다.");
        }

        // 고스트 생성 효과음
        if (AudioManager.I != null)
            AudioManager.I.GhostSpawn(go.transform.position);

        usesRemaining[slot]--;

        Debug.Log($"Spawn Ghost {slot + 1} (uses left: {usesRemaining[slot]})");
    }

    void PlayDespawnVoiceForGhost(GameObject ghost)
    {
        if (ghost == null)
            return;

        GhostVoiceActor voiceActor = ghost.GetComponent<GhostVoiceActor>();

        if (voiceActor != null)
        {
            voiceActor.PlayDespawnVoice();
            return;
        }

        if (GhostChapterVoiceBank.I != null)
            GhostChapterVoiceBank.I.PlayGhostDespawnVoice(ghost.transform.position);
    }

    void ClearAllSlotsAndGhosts()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject ghost = activeGhost[i];

            if (ghost != null)
            {
                PlayDespawnVoiceForGhost(ghost);
                Destroy(ghost);
            }

            activeGhost[i] = null;
        }

        for (int i = 0; i < 3; i++)
        {
            recording[i] = false;
            recordTimer[i] = 0f;
            usesRemaining[i] = 0;

            if (clips[i] != null)
                clips[i].Clear();
        }

        interactQueued = false;
        jumpQueued = false;
    }

    void ResetStage()
    {
        ClearAllSlotsAndGhosts();
        SetRecUI(false);

        int idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    void SetRecUI(bool on)
    {
        if (recGroup != null)
        {
            recGroup.alpha = on ? 1f : 0f;
            recGroup.blocksRaycasts = false;
            recGroup.interactable = false;
        }

        if (!on)
        {
            if (recDot != null)
                recDot.enabled = false;

            if (recTimerText != null)
                recTimerText.text = "";

            return;
        }

        if (recDot != null)
        {
            float t = Time.unscaledTime * blinkHz;
            bool visible = (Mathf.FloorToInt(t) % 2) == 0;
            recDot.enabled = visible;
        }

        if (recTimerText != null)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 3; i++)
            {
                if (!recording[i])
                    continue;

                if (sb.Length > 0)
                    sb.Append('\n');

                sb.Append($"REC {i + 1}  {recordTimer[i]:0.0}s");
            }

            recTimerText.text = sb.ToString();
        }
    }
}