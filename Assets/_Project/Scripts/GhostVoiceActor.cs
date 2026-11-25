using UnityEngine;

public class GhostVoiceActor : MonoBehaviour
{
    [SerializeField] bool playSpawnVoiceOnEnable = true;
    [SerializeField] bool spawnOnlyOnce = true;

    bool spawnedVoicePlayed = false;

    void OnEnable()
    {
        if (!playSpawnVoiceOnEnable)
            return;

        if (spawnOnlyOnce && spawnedVoicePlayed)
            return;

        if (GhostChapterVoiceBank.I != null)
            GhostChapterVoiceBank.I.PlayGhostSpawnVoiceOnly(transform.position);
        spawnedVoicePlayed = true;
    }

    public void PlayDespawnVoice()
    {
        if (GhostChapterVoiceBank.I != null)
            GhostChapterVoiceBank.I.PlayGhostDespawnVoice(transform.position);
    }
}