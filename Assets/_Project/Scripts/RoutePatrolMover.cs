using UnityEngine;

public class RoutePatrolMover : MonoBehaviour
{
    [Header("Waypoints (순서대로)")]
    public Transform[] waypoints;

    [Header("Move")]
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.2f;
    public bool startFromClosestPoint = false;

    private Vector3[] worldPoints;
    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;

    private Vector3 lastPosition;
    public Vector3 FrameDelta { get; private set; }

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning($"[{name}] waypoints가 2개 이상 필요함");
            enabled = false;
            return;
        }

        worldPoints = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogWarning($"[{name}] waypoints[{i}] 가 비어 있음");
                enabled = false;
                return;
            }

            worldPoints[i] = waypoints[i].position;
        }

        if (startFromClosestPoint)
            currentIndex = FindClosestWaypointIndex();

        transform.position = worldPoints[currentIndex];
        lastPosition = transform.position;
        FrameDelta = Vector3.zero;
    }

    void Update()
    {
        if (worldPoints == null || worldPoints.Length < 2)
        {
            FrameDelta = Vector3.zero;
            return;
        }

        Vector3 before = transform.position;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            Vector3 target = worldPoints[currentIndex];

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, target) <= 0.01f)
            {
                waitTimer = waitTimeAtPoint;
                SetNextIndexPingPong();
            }
        }

        FrameDelta = transform.position - before;
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        // 다음 프레임 전 기본값 유지용
        if (transform.position == lastPosition)
            FrameDelta = Vector3.zero;
    }

    void SetNextIndexPingPong()
    {
        currentIndex += direction;

        if (currentIndex >= worldPoints.Length)
        {
            direction = -1;
            currentIndex = worldPoints.Length - 2;
        }
        else if (currentIndex < 0)
        {
            direction = 1;
            currentIndex = 1;
        }
    }

    int FindClosestWaypointIndex()
    {
        int closest = 0;
        float bestDist = Mathf.Infinity;

        for (int i = 0; i < worldPoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, worldPoints[i]);
            if (d < bestDist)
            {
                bestDist = d;
                closest = i;
            }
        }

        return closest;
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.12f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}