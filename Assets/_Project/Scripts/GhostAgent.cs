using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GhostAgent : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.4f;

    CharacterController cc;
    PlayerInteractor interactor;

    List<GhostFrame> frames;
    int index = 0;
    float verticalVelocity = 0f;

    bool despawnVoicePlayed = false;

    public void Init(List<GhostFrame> recordedFrames)
    {
        frames = new List<GhostFrame>(recordedFrames);
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        interactor = GetComponent<PlayerInteractor>();

        if (interactor != null)
            interactor.listenToInput = false;
    }

    void FixedUpdate()
    {
        if (frames == null || frames.Count == 0 || index >= frames.Count)
        {
            PlayDespawnVoiceOnce();
            Destroy(gameObject);
            return;
        }

        float dt = Time.fixedDeltaTime;

        GhostFrame f = frames[index];
        index++;

        Vector3 move = new Vector3(f.move.x, 0f, f.move.y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (cc.isGrounded && f.jump)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * dt;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * dt);

        if (f.interact && interactor != null)
            interactor.TryInteract();
    }

    void PlayDespawnVoiceOnce()
    {
        if (despawnVoicePlayed)
            return;

        despawnVoicePlayed = true;

        GhostVoiceActor voiceActor = GetComponent<GhostVoiceActor>();

        if (voiceActor != null)
        {
            voiceActor.PlayDespawnVoice();
            return;
        }

        if (GhostChapterVoiceBank.I != null)
        {
            GhostChapterVoiceBank.I.PlayGhostDespawnVoice(transform.position);
        }
    }
}