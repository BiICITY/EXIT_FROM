using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RealFirstPersonController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3.2f;
    public float gravity = -20f;

    [Header("Keyboard Turn")]
    [Tooltip("A/D 또는 좌우 방향키로 시점을 돌리는 속도")]
    public float keyboardTurnSpeed = 120f;

    [Header("Mouse Look")]
    public bool useMouseLook = true;
    public Transform cameraSocket;
    public float mouseSensitivity = 2f;
    public float minPitch = -70f;
    public float maxPitch = 70f;

    private CharacterController cc;
    private float verticalVelocity;
    private float pitch;
    private bool canControl = false;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!canControl) return;

        Look();
        Move();
    }

    public void SetControl(bool value)
    {
        canControl = value;

        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !value;
    }

    public void SetViewRotation(float yaw, float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraSocket != null)
            cameraSocket.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public float GetCurrentYaw()
    {
        return transform.eulerAngles.y;
    }

    public float GetCurrentPitch()
    {
        return pitch;
    }

    private void Look()
    {
        float turnInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            turnInput -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            turnInput += 1f;

        // 키보드 좌우 입력으로 몸 전체 회전
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            transform.Rotate(Vector3.up * turnInput * keyboardTurnSpeed * Time.deltaTime);
        }

        // 마우스도 같이 시점 회전에 사용하고 싶을 때
        if (useMouseLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (cameraSocket != null)
                cameraSocket.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void Move()
    {
        float forwardInput = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            forwardInput += 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            forwardInput -= 1f;

        // 현재 바라보는 방향으로만 전진/후진
        Vector3 move = transform.forward * forwardInput;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);
    }
}