using UnityEngine;
using System.Collections;

public class PlayerDeathController : MonoBehaviour
{
    [Header("Respawn (Manual Input)")]
    public bool useManualRespawn = true;

    [Tooltip("리스폰 월드 좌표를 직접 입력")]
    public Vector3 respawnPosition;

    [Tooltip("리스폰 회전(Euler) 직접 입력")]
    public Vector3 respawnEuler;

    [Header("Timing")]
    public float respawnDelay = 0f;
    public float deathCooldown = 0.15f;

    [Header("Optional: disable these during respawn")]
    public MonoBehaviour[] disableWhileDead;

    [Header("Audio")]
    [Tooltip("true면 Die() 호출 시 사운드 재생")]
    public bool playDeathSfx = true;

    [Tooltip("레이저/낙사 구분 없이 같은 사운드 써도 OK. (AudioManager에 클립이 없으면 자동으로 아무것도 안 남)")]
    public bool useSameSfxForAllDeath = true;

    Vector3 startPos;
    Quaternion startRot;

    float lastDeathTime = -999f;
    bool isDead;

    CharacterController cc;
    Rigidbody rb;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    public void Die(string reason = "")
    {
        if (isDead) return;
        if (Time.time - lastDeathTime < deathCooldown) return;

        lastDeathTime = Time.time;

        // ✅ 죽는 순간 즉시 사운드 1회 재생
        if (playDeathSfx)
            PlayDeathSound(reason);

        StartCoroutine(DieRoutine(reason));
    }

    void PlayDeathSound(string reason)
    {
        if (AudioManager.I == null) return;

        // 같은 사운드로 통일하고 싶다면 Laser로만 재생(또는 Fall로만 재생)
        if (useSameSfxForAllDeath)
        {
            AudioManager.I.DeathLaser(transform.position);
            return;
        }

        // 구분하고 싶다면 reason으로 분기
        // (네 프로젝트에서 reason이 "LaserBeam" / "Fall" 이런 식으로 들어오면 맞춰서 사용)
        if (!string.IsNullOrEmpty(reason) && reason.ToLower().Contains("fall"))
            AudioManager.I.DeathFall(transform.position);
        else
            AudioManager.I.DeathLaser(transform.position);
    }

    IEnumerator DieRoutine(string reason)
    {
        isDead = true;

        if (disableWhileDead != null)
            foreach (var mb in disableWhileDead)
                if (mb != null) mb.enabled = false;

        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        RespawnNow();

        if (disableWhileDead != null)
            foreach (var mb in disableWhileDead)
                if (mb != null) mb.enabled = true;

        isDead = false;
    }

    public void RespawnNow()
    {
        Vector3 pos = useManualRespawn ? respawnPosition : startPos;
        Quaternion rot = useManualRespawn ? Quaternion.Euler(respawnEuler) : startRot;

        if (cc != null) cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        if (cc != null) cc.enabled = true;

        if (rb != null)
        {
            // ✅ 경고 방지: kinematic일 수 있으니 체크
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // kinematic이면 velocity 설정 경고가 뜰 수 있어서 회피
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    [ContextMenu("Set Respawn To Current Transform")]
    public void SetRespawnToCurrent()
    {
        respawnPosition = transform.position;
        respawnEuler = transform.rotation.eulerAngles;
    }
}
