using System.Collections;
using UnityEngine;

public class SwapWithGhost : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swapKey = KeyCode.Z;
    public float cooldown = 0.5f;

    [Header("Ghost Search")]
    public string ghostTag = "Ghost";
    public float maxDistance = 999f;

    [Header("Safety (optional)")]
    public bool preventSwapIntoWall = true;

    [Tooltip("벽/바닥/고정 장애물 레이어만 체크 (Player/Ghost/Possessable은 보통 빼기)")]
    public LayerMask obstacleMask = ~0;

    [Tooltip("고스트는 유령이라 겹쳐도 된다면 true 추천 (고스트 쪽 충돌 체크 생략)")]
    public bool ignoreGhostCollisionCheck = true;

    [Header("Pass Through (핵심)")]
    [Tooltip("스왑 순간에만 플레이어-고스트 충돌을 무시해서 서로 통과")]
    public bool ignorePlayerGhostCollisionDuringSwap = true;

    [Tooltip("충돌 무시 지속 시간(초). 0.1~0.25 추천")]
    public float collisionIgnoreSeconds = 0.15f;

    [Header("Optional: stop ghost movement after swap")]
    public bool disableGhostMovementAfterSwap = false;

    [Tooltip("고스트를 움직이는 Behaviour(Animator 포함)를 여기 넣으면 스왑 후 꺼짐")]
    public Behaviour[] ghostMovementBehaviours;

    [Header("VFX")]
    public GameObject swapVfxPrefab;
    public float vfxLifeTime = 2f;
    public Vector3 vfxOffset = Vector3.up * 0.8f;

    [Header("Camera (FollowCamera on rig)")]
    public FollowCamera followCamera;   // CameraRig의 FollowCamera 드래그 추천
    public Transform playerCamTarget;   // 비워두면 PlayerCamTarget 또는 Player로 자동
    public bool snapCameraAfterSwap = true;

    [Header("Debug")]
    public bool debugLogs = true;

    float lastSwapTime = -999f;
    CharacterController playerCC;

    // 충돌 체크 버퍼(할당 줄이기)
    readonly Collider[] overlapBuf = new Collider[32];

    // 플레이어 콜라이더 캐시
    Collider[] playerCols;

    void Awake()
    {
        playerCC = GetComponent<CharacterController>();
        playerCols = GetComponentsInChildren<Collider>(true); // CharacterController 포함
    }

    void Start()
    {
        if (followCamera == null)
            followCamera = FindObjectOfType<FollowCamera>(true);

        if (playerCamTarget == null)
        {
            var t = transform.Find("PlayerCamTarget");
            playerCamTarget = (t != null) ? t : transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(swapKey))
            TrySwap();
    }

    void TrySwap()
    {
        if (Time.time - lastSwapTime < cooldown)
        {
            if (debugLogs) Debug.Log("Swap 실패: cooldown");
            return;
        }

        Transform ghost = FindNearestGhost();
        if (ghost == null)
        {
            if (debugLogs) Debug.LogWarning("Swap 실패: 활성 Ghost 없음(태그 Ghost 확인)");
            return;
        }

        float dist = Vector3.Distance(transform.position, ghost.position);
        if (dist > maxDistance)
        {
            if (debugLogs) Debug.Log($"Swap 실패: too far ({dist:0.0} > {maxDistance})");
            return;
        }

        CharacterController ghostCC = ghost.GetComponent<CharacterController>();

        Vector3 playerPos = transform.position;
        Vector3 ghostPos = ghost.position;

        // 안전 스왑(벽 속 방지)
        if (preventSwapIntoWall)
        {
            if (!CanStandAt(ghostPos, transform, playerCC))
            {
                if (debugLogs) Debug.Log("Swap 실패: 플레이어가 고스트 위치에 설 수 없음(벽/장애물)");
                return;
            }

            if (!ignoreGhostCollisionCheck)
            {
                if (!CanStandAt(playerPos, ghost, ghostCC))
                {
                    if (debugLogs) Debug.Log("Swap 실패: 고스트가 플레이어 위치에 설 수 없음(벽/장애물)");
                    return;
                }
            }
        }

        if (debugLogs) Debug.Log($"Swap SUCCESS target={ghost.name} dist={dist:0.00}");

        // VFX
        SpawnVfx(playerPos);
        SpawnVfx(ghostPos);

        // 고스트 이동 스크립트 끄기(필요할 때만)
        if (disableGhostMovementAfterSwap && ghostMovementBehaviours != null)
        {
            for (int i = 0; i < ghostMovementBehaviours.Length; i++)
                if (ghostMovementBehaviours[i] != null) ghostMovementBehaviours[i].enabled = false;
        }

        // ✅ 스왑 순간에만 서로 통과(충돌 무시)
        Collider[] ghostCols = null;
        if (ignorePlayerGhostCollisionDuringSwap)
        {
            ghostCols = ghost.GetComponentsInChildren<Collider>(true);
            SetIgnoreCollisions(playerCols, ghostCols, true);
            StartCoroutine(RestoreCollisionAfter(collisionIgnoreSeconds, playerCols, ghostCols));
        }

        // CC는 순간이동 전에 껐다 켜는 게 안정적
        if (playerCC != null) playerCC.enabled = false;
        if (ghostCC != null) ghostCC.enabled = false;

        // ✅ 진짜 “서로” 위치 교환
        transform.position = ghostPos;
        ghost.position = playerPos;

        if (playerCC != null) playerCC.enabled = true;
        if (ghostCC != null) ghostCC.enabled = true;

        lastSwapTime = Time.time;

        // 카메라 타겟은 플레이어로(상자 조종 등으로 바뀌어있을 수 있으니)
        if (followCamera != null)
        {
            followCamera.target = playerCamTarget;

            if (snapCameraAfterSwap)
            {
                followCamera.transform.position = followCamera.target.position + followCamera.offset;
                followCamera.transform.LookAt(followCamera.target.position + followCamera.lookOffset);
            }
        }
    }

    Transform FindNearestGhost()
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag(ghostTag);
        if (ghosts == null || ghosts.Length == 0) return null;

        Transform best = null;
        float bestD = float.MaxValue;
        Vector3 p = transform.position;

        for (int i = 0; i < ghosts.Length; i++)
        {
            var g = ghosts[i];
            if (g == null || !g.activeInHierarchy) continue;

            float d = (g.transform.position - p).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = g.transform;
            }
        }
        return best;
    }

    void SpawnVfx(Vector3 pos)
    {
        if (swapVfxPrefab == null) return;
        var go = Instantiate(swapVfxPrefab, pos + vfxOffset, Quaternion.identity);
        Destroy(go, Mathf.Max(0.1f, vfxLifeTime));
    }

    bool CanStandAt(Vector3 targetPos, Transform rootToIgnore, CharacterController cc)
    {
        float radius = (cc != null) ? Mathf.Max(0.05f, cc.radius) : 0.35f;
        float height = (cc != null) ? Mathf.Max(radius * 2f, cc.height) : 1.8f;

        Vector3 bottom = targetPos + Vector3.up * radius;
        Vector3 top = targetPos + Vector3.up * (height - radius);

        int n = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlapBuf, obstacleMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < n; i++)
        {
            var c = overlapBuf[i];
            if (c == null) continue;

            if (c.transform == rootToIgnore || c.transform.IsChildOf(rootToIgnore)) continue;
            if (c.isTrigger) continue;

            return false;
        }
        return true;
    }

    void SetIgnoreCollisions(Collider[] a, Collider[] b, bool ignore)
    {
        if (a == null || b == null) return;

        for (int i = 0; i < a.Length; i++)
        {
            var ca = a[i];
            if (ca == null) continue;

            for (int j = 0; j < b.Length; j++)
            {
                var cb = b[j];
                if (cb == null) continue;

                // 트리거는 굳이 무시해도 상관없지만, 안전하게 포함해도 됨
                Physics.IgnoreCollision(ca, cb, ignore);
            }
        }
    }

    IEnumerator RestoreCollisionAfter(float seconds, Collider[] a, Collider[] b)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        SetIgnoreCollisions(a, b, false);
    }
}
