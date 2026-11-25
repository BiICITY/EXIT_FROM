using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FinalExitTrigger : MonoBehaviour
{
    [Header("End")]
    public bool loadNextScene = false;
    public string nextSceneName = "";

    [Header("End Message UI")]
    public GameObject endPanel;
    public TextMeshProUGUI endText;
    public string endMessage = "EXIT_FROM END";
    public float endHoldTime = 3f;

    private bool triggered = false;

    private void Start()
    {
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        RealFirstPersonController realPlayer = other.GetComponentInParent<RealFirstPersonController>();
        if (realPlayer == null) return;

        triggered = true;
        realPlayer.SetControl(false);

        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        if (endPanel != null)
            endPanel.SetActive(true);

        if (endText != null)
            endText.text = endMessage;

        Debug.Log(endMessage);

        yield return new WaitForSeconds(endHoldTime);

        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Application.Quit();
        }
    }
}