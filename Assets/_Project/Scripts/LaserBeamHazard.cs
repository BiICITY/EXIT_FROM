using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamHazard : MonoBehaviour
{
    [Header("Movement (optional)")]
    public bool moveBetweenPoints = true;
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;

    [Header("Beam")]
    public Transform origin;                      // 비우면 this.transform
    public Vector3 localDirection = Vector3.right; // 로컬 방향(오른쪽/앞/위 중 맞게)
    public float maxDistance = 30f;

    [Tooltip("0이면 Raycast(얇음), 0.1~0.3이면 SphereCast(두꺼워져서 상자로 막기 쉬움)")]
    public float radius = 0.2f;

    [Tooltip("레이저가 맞을 레이어(맵/상자/플레이어 포함)")]
    public LayerMask hitMask;

    [Header("Kill")]
    public string playerTag = "Player";
    public float hitCooldown = 0.15f;

    LineRenderer lr;
    float lastHitTime = -999f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        if (origin == null) origin = transform;
    }

    void Update()
    {
        // 1) 이동
        if (moveBetweenPoints && pointA != null && pointB != null)
        {
            float t = Mathf.PingPong(Time.time * moveSpeed, 1f);
            transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
        }

        // 2) 빔 쏘기(첫 충돌에서 끊김)
        Vector3 o = origin.position;
        Vector3 dir = origin.TransformDirection(localDirection).normalized;

        Vector3 end = o + dir * maxDistance;

        RaycastHit hit;
        bool hasHit = false;

        if (radius > 0f)
            hasHit = Physics.SphereCast(o, radius, dir, out hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore);
        else
            hasHit = Physics.Raycast(o, dir, out hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore);

        if (hasHit)
        {
            end = hit.point;

            // 첫 타격이 플레이어면 즉사
            if (hit.collider != null && hit.collider.CompareTag(playerTag))
            {
                if (Time.time - lastHitTime >= hitCooldown)
                {
                    lastHitTime = Time.time;
                    var death = hit.collider.GetComponentInParent<PlayerDeathController>();
                    death?.Die("LaserBeam");
                }
            }
            // 첫 타격이 상자면 여기서 끝(= 막힘) → 플레이어는 안전
        }

        // 3) 라인렌더 표시
        lr.SetPosition(0, o);
        lr.SetPosition(1, end);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (origin == null) origin = transform;
        Vector3 o = origin.position;
        Vector3 dir = origin.TransformDirection(localDirection).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(o, o + dir * 2f);
    }
#endif
}
