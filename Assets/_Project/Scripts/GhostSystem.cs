using System.Collections.Generic;
using UnityEngine;

public class GhostSystem : MonoBehaviour
{
    public static GhostSystem Instance { get; private set; }

    [Header("Recording")]
    public float recordDuration = 8f;

    [Header("References")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public GameObject ghostPrefab;

    public bool HasClip => recordedFrames != null && recordedFrames.Count > 0;
    public bool IsRecording => isRecording;

    private bool isRecording = false;
    private float recordTimer = 0f;

    private List<GhostFrame> recordedFrames = new List<GhostFrame>();

    // FixedUpdate에서 사용할 입력 캐시
    private Vector2 cachedMove;
    private bool interactQueued = false;

    // 고스트 시작 위치(Record 시작 위치)
    private Vector3 recordStartPos;
    private Quaternion recordStartRot;

    private GameObject currentGhost;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 자동 참조(안 넣어도 되게)
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerInteractor == null) playerInteractor = FindFirstObjectByType<PlayerInteractor>();
    }

    void Update()
    {
        // 이동 입력 캐시
        cachedMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (cachedMove.sqrMagnitude > 1f) cachedMove.Normalize();

        // 상호작용 입력 캐시(한 번 누르면 다음 FixedUpdate에서 소비)
        if (Input.GetKeyDown(KeyCode.E))
            interactQueued = true;
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        recordTimer += Time.fixedDeltaTime;

        var f = new GhostFrame
        {
            move = cachedMove,
            interact = interactQueued
        };
        interactQueued = false;

        recordedFrames.Add(f);

        if (recordTimer >= recordDuration)
        {
            StopRecording();
        }
    }

    public void StartRecordingFromPad(Transform recordPadTransform)
    {
        if (isRecording) return;
        if (playerController == null) return;

        recordedFrames.Clear();
        recordTimer = 0f;
        isRecording = true;

        recordStartPos = playerController.transform.position;
        recordStartRot = playerController.transform.rotation;

        Debug.Log("Recording started");
    }

    private void StopRecording()
    {
        isRecording = false;
        recordTimer = 0f;
        interactQueued = false;

        Debug.Log($"Recording finished. Frames: {recordedFrames.Count}");
    }

    public void SpawnGhost()
    {
        if (isRecording) return;
        if (!HasClip) return;
        if (ghostPrefab == null) return;

        // 1개만 유지
        if (currentGhost != null)
            Destroy(currentGhost);

        currentGhost = Instantiate(ghostPrefab, recordStartPos, recordStartRot);

        var agent = currentGhost.GetComponent<GhostAgent>();
        if (agent != null)
        {
            // 플레이어 설정과 비슷하게 맞추고 싶으면 복사
            if (playerController != null)
            {
                agent.moveSpeed = playerController.moveSpeed;
                agent.gravity = playerController.gravity;
            }
            agent.Init(recordedFrames);
        }
    }
}
