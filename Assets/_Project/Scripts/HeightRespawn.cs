using UnityEngine;

public class HeightRespawn : MonoBehaviour
{
    [Header("Check")]
    [Tooltip("이 높이 이상 올라가면 리스폰")]
    public float maxY = 20f;

    [Header("Respawn")]
    [Tooltip("비워두면 아래 manualRespawnPosition 사용")]
    public Transform respawnPoint;

    [Tooltip("respawnPoint를 비웠을 때 사용할 좌표")]
    public Vector3 manualRespawnPosition = Vector3.zero;

    [Tooltip("리스폰할 때 회전도 같이 맞출지")]
    public bool applyRespawnRotation = false;

    [Tooltip("respawnPoint를 비웠을 때 사용할 회전")]
    public Vector3 manualRespawnEuler = Vector3.zero;

    [Header("Optional Reset")]
    public bool resetRigidbodyVelocity = true;
    public bool disableCharacterControllerDuringTeleport = true;

    CharacterController cc;
    Rigidbody rb;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y >= maxY)
        {
            RespawnNow();
        }
    }

    public void RespawnNow()
    {
        Vector3 targetPos = respawnPoint != null ? respawnPoint.position : manualRespawnPosition;
        Quaternion targetRot = respawnPoint != null
            ? respawnPoint.rotation
            : Quaternion.Euler(manualRespawnEuler);

        if (disableCharacterControllerDuringTeleport && cc != null)
            cc.enabled = false;

        transform.position = targetPos;

        if (applyRespawnRotation)
            transform.rotation = targetRot;

        if (disableCharacterControllerDuringTeleport && cc != null)
            cc.enabled = true;

        if (resetRigidbodyVelocity && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}