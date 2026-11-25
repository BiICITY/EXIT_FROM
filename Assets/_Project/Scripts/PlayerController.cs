using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float gravity = -20f;

    [Header("Jump")]
    public float jumpHeight = 1.4f;

    [Header("Rotation")]
    [Tooltip("이동 방향으로 캐릭터가 회전할지")]
    public bool rotateToMoveDirection = true;

    [Tooltip("회전 속도. 높을수록 빠르게 방향 전환")]
    public float rotationSpeed = 12f;

    [Tooltip("실제 외형만 돌리고 싶으면 Body/Visual 루트를 넣기. 비워두면 Player 전체가 회전")]
    public Transform visualRoot;

    private CharacterController cc;
    private float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (visualRoot == null)
            visualRoot = transform;
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))  h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.DownArrow))  v -= 1f;
        if (Input.GetKey(KeyCode.UpArrow))    v += 1f;

        Vector3 move = new Vector3(h, 0f, v);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        RotateToMoveDirection(move);

        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (cc.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            AudioManager.I?.Jump();
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);
    }

    void RotateToMoveDirection(Vector3 move)
    {
        if (!rotateToMoveDirection) return;
        if (visualRoot == null) return;

        // 입력이 없으면 마지막 방향 유지
        if (move.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);

        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = -2f;
    }
}