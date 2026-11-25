using UnityEngine;

public class LaserMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    void Update()
    {
        if (pointA == null || pointB == null) return;

        float t = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
    }
}
