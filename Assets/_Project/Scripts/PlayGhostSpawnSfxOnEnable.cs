using UnityEngine;

public class PlayGhostSpawnSfxOnEnable : MonoBehaviour
{
    [SerializeField] bool playOnlyOnce = true;
    bool played = false;

    void OnEnable()
    {
        if (playOnlyOnce && played) return;

        AudioManager.I?.GhostSpawn(transform.position);
        played = true;
    }
}