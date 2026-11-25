using UnityEngine;

public class FinalGoalTrigger : MonoBehaviour
{
    [Header("Final Ending")]
    public FinalEndingDirector endingDirector;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        triggered = true;

        if (endingDirector != null)
        {
            endingDirector.StartEnding();
        }
        else
        {
            Debug.LogWarning("[FinalGoalTrigger] Ending Director가 연결되지 않았습니다.");
        }
    }
}