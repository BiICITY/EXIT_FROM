using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Tooltip("비워두면 시작할 때 target을 기본 타겟으로 저장")]
    public Transform defaultTarget;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 8f, -8f);
    public float followSpeed = 10f;

    [Header("Look")]
    public Vector3 lookOffset = Vector3.zero; // 필요하면 (0,1,0) 같은 값

    void Awake()
    {
        if (defaultTarget == null) defaultTarget = target;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        // 항상 타겟을 바라보기
        transform.LookAt(target.position + lookOffset);
    }

    /// <summary>카메라가 따라갈 타겟을 바꿈</summary>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;
        target = newTarget;
    }

    /// <summary>기본 타겟(보통 플레이어)으로 복귀</summary>
    public void ResetToDefault()
    {
        if (defaultTarget != null) target = defaultTarget;
    }
}
