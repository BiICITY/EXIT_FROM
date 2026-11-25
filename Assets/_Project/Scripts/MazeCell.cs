using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public float upY = 1f;
    public float downY = -3f;
    public float moveSpeed = 4f;

    private Vector3 targetPosition;

    private void Awake()
    {
        targetPosition = transform.position;
    }

    public void SetWall(bool isWall)
    {
        Vector3 p = transform.position;
        p.y = isWall ? upY : downY;
        targetPosition = p;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}