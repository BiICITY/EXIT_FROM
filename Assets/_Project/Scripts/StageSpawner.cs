using UnityEngine;

public class StageSpawner : MonoBehaviour
{
    [Header("Anchors")]
    public Transform startPoint;
    public Transform goalPoint;

    [Header("Prefabs")]
    public GameObject goalPrefab; // 없으면 씬에 이미 있는 Goal을 사용해도 됨

    [Header("Options")]
    public bool movePlayerToStartOnLoad = true;
    public bool spawnGoalIfMissing = true;

    void Start()
    {
        if (movePlayerToStartOnLoad)
            MovePlayerToStart();

        if (spawnGoalIfMissing)
            EnsureGoal();
    }

    void MovePlayerToStart()
    {
        if (startPoint == null) return;

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // CharacterController가 있으면 끄고 옮기는게 안전
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = startPoint.position;
        player.transform.rotation = startPoint.rotation;

        if (cc != null) cc.enabled = true;
    }

    void EnsureGoal()
    {
        if (goalPoint == null) return;

        // 이미 GoalZone이 있으면 생성 안 함
        var existing = FindFirstObjectByType<GoalZone>();
        if (existing != null) return;

        if (goalPrefab == null)
        {
            Debug.LogWarning("StageSpawner: goalPrefab is null. (Assign a Goal prefab or place Goal manually.)");
            return;
        }

        Instantiate(goalPrefab, goalPoint.position, goalPoint.rotation);
    }
}
