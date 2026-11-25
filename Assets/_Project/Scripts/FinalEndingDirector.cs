using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalEndingDirector : MonoBehaviour
{
    [Header("Old Gameplay")]
    [Tooltip("마지막 연출 시작 시 꺼둘 컴포넌트들. PlayerController, PlayerInteractor 등을 넣기")]
    public MonoBehaviour[] disableOnEndingStart;

    [Tooltip("CameraRig에 붙어있는 카메라 추적 스크립트")]
    public MonoBehaviour cameraFollowScript;

    [Header("Optional: Disable Other Scripts")]
    [Tooltip("체크하면 엔딩 시작 시 씬 안의 다른 MonoBehaviour들을 대부분 꺼버림. 처음에는 꺼두고 테스트 추천")]
    public bool disableOtherSceneScripts = false;

    [Tooltip("자동 중지에서 제외할 스크립트들. AudioManager, EventSystem 관련, FinalExitTrigger 등을 필요하면 넣기")]
    public MonoBehaviour[] keepEnabledScripts;

    [Header("Camera Move")]
    public Transform cameraRig;
    public Camera mainCamera;
    public Transform cameraArrivePoint;

    [Tooltip("카메라가 연구실 시점으로 이동하는 시간")]
    public float cameraMoveTime = 5f;

    [Tooltip("카메라가 CameraArrivePoint에 도착한 뒤 잠깐 멈추는 시간")]
    public float revealHoldTime = 1.5f;

    [Header("First Person Camera Offset")]
    [Tooltip("Main Camera가 CameraSocket 자식으로 들어온 뒤의 로컬 위치")]
    public Vector3 firstPersonCameraLocalPosition = new Vector3(0f, 0.05f, -0.45f);

    [Tooltip("Main Camera가 CameraSocket 자식으로 들어온 뒤의 로컬 회전")]
    public Vector3 firstPersonCameraLocalEuler = Vector3.zero;

    [Tooltip("카메라가 붙은 뒤에도 매 프레임 이 위치를 강제 유지")]
    public bool forceFirstPersonCameraOffset = true;

    [Header("Turn After Attach")]
    [Tooltip("카메라가 RealPlayer에 붙은 뒤 RealPlayer를 왼쪽으로 회전")]
    public bool turnLeftAfterAttach = true;

    [Tooltip("왼쪽 90도면 -90. 반대로 돌면 90으로 바꾸기")]
    public float turnLeftAngle = -90f;

    [Tooltip("회전을 부드럽게 할지")]
    public bool smoothTurnAfterAttach = true;

    [Tooltip("부드럽게 회전하는 시간")]
    public float turnAfterAttachTime = 1.5f;

    [Tooltip("회전 후 잠깐 멈추는 시간")]
    public float afterTurnHoldTime = 0.4f;

    [Header("Real Player")]
    public GameObject realPlayerRoot;
    public RealFirstPersonController realPlayerController;
    public Transform realCameraSocket;

    [Header("Researcher")]
    public Transform researcher;
    public Transform researcherArrivePoint;
    public float researcherMoveSpeed = 1.2f;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public float dialogueHoldTime = 2.5f;

    [Header("Exit")]
    public GameObject exitGuideObject;

    private bool started = false;
    private bool cameraAttachedToRealPlayer = false;

    private readonly List<MonoBehaviour> autoDisabledScripts = new List<MonoBehaviour>();

    private void LateUpdate()
    {
        if (!forceFirstPersonCameraOffset) return;
        if (!cameraAttachedToRealPlayer) return;
        if (mainCamera == null) return;
        if (realCameraSocket == null) return;

        if (mainCamera.transform.parent != realCameraSocket)
            return;

        mainCamera.transform.localPosition = firstPersonCameraLocalPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(firstPersonCameraLocalEuler);
    }

    public void StartEnding()
    {
        if (started) return;

        started = true;
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // 1. RealPlayer 켜기
        if (realPlayerRoot != null)
            realPlayerRoot.SetActive(true);

        // 2. RealPlayer 조작은 아직 금지
        if (realPlayerController != null)
            realPlayerController.SetControl(false);

        // 3. 출구 안내 숨김
        if (exitGuideObject != null)
            exitGuideObject.SetActive(false);

        HideDialogue();

        // 4. 직접 지정한 기존 플레이 스크립트 끄기
        DisableManualScripts();

        // 5. 카메라 추적 스크립트 끄기
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        // 6. 필요하면 다른 씬 스크립트들도 자동으로 끄기
        if (disableOtherSceneScripts)
            DisableOtherScriptsSafely();

        // 7. 카메라 이동
        yield return MoveCameraRigToArrivePoint();

        // 8. 반전 장면 잠깐 보여주기
        if (revealHoldTime > 0f)
            yield return new WaitForSeconds(revealHoldTime);

        // 9. Main Camera를 RealPlayer의 CameraSocket 아래로 붙임
        AttachCameraToRealPlayer();

        // 10. 붙은 뒤 RealPlayer와 카메라가 같이 왼쪽 90도 회전
        if (turnLeftAfterAttach)
        {
            if (smoothTurnAfterAttach)
                yield return TurnRealPlayerAfterAttach();
            else
                TurnRealPlayerInstant();
        }

        if (afterTurnHoldTime > 0f)
            yield return new WaitForSeconds(afterTurnHoldTime);

        // 11. 연구원 이동
        yield return MoveResearcherToPlayer();

        // 12. 연구원 대사
        yield return ShowDialogue("The experiment is now complete.");
        yield return ShowDialogue("The results were not meaningful enough to continue.");
        yield return ShowDialogue("But the procedure ended without incident.");
        yield return ShowDialogue("You may go home now.");
        yield return ShowDialogue("The exit is over there.");

        HideDialogue();

        // 13. 출구 안내 켜기
        if (exitGuideObject != null)
            exitGuideObject.SetActive(true);

        // 14. RealPlayer 조작 가능
        if (realPlayerController != null)
            realPlayerController.SetControl(true);
    }

    private void DisableManualScripts()
    {
        if (disableOnEndingStart == null) return;

        for (int i = 0; i < disableOnEndingStart.Length; i++)
        {
            if (disableOnEndingStart[i] != null)
                disableOnEndingStart[i].enabled = false;
        }
    }

    private void DisableOtherScriptsSafely()
    {
        autoDisabledScripts.Clear();

#if UNITY_6000_0_OR_NEWER
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
#else
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
#endif

        for (int i = 0; i < allBehaviours.Length; i++)
        {
            MonoBehaviour mb = allBehaviours[i];

            if (mb == null) continue;
            if (!mb.enabled) continue;

            // 자기 자신은 절대 끄면 안 됨
            if (mb == this) continue;

            // RealPlayer 조작 스크립트는 나중에 켜야 하므로 끄지 않음
            if (realPlayerController != null && mb == realPlayerController) continue;

            // 수동으로 보존할 스크립트들
            if (IsInKeepList(mb)) continue;

            // Main Camera 자체에 붙은 카메라 컴포넌트는 MonoBehaviour가 아니라 Camera라 여기 안 들어옴
            // AudioManager, StageUIController 같은 싱글톤은 keepEnabledScripts에 넣는 것을 추천

            mb.enabled = false;
            autoDisabledScripts.Add(mb);
        }
    }

    private bool IsInKeepList(MonoBehaviour mb)
    {
        if (keepEnabledScripts == null) return false;

        for (int i = 0; i < keepEnabledScripts.Length; i++)
        {
            if (keepEnabledScripts[i] == mb)
                return true;
        }

        return false;
    }

    private IEnumerator MoveCameraRigToArrivePoint()
    {
        if (cameraRig == null || cameraArrivePoint == null)
        {
            Debug.LogWarning("[FinalEndingDirector] CameraRig 또는 CameraArrivePoint가 비어 있습니다.");
            yield break;
        }

        Vector3 startPos = cameraRig.position;
        Quaternion startRot = cameraRig.rotation;

        Vector3 endPos = cameraArrivePoint.position;
        Quaternion endRot = cameraArrivePoint.rotation;

        float t = 0f;

        while (t < cameraMoveTime)
        {
            t += Time.deltaTime;

            float a = Mathf.Clamp01(t / cameraMoveTime);
            a = Mathf.SmoothStep(0f, 1f, a);

            cameraRig.position = Vector3.Lerp(startPos, endPos, a);
            cameraRig.rotation = Quaternion.Slerp(startRot, endRot, a);

            yield return null;
        }

        cameraRig.position = endPos;
        cameraRig.rotation = endRot;
    }

    private void AttachCameraToRealPlayer()
    {
        if (mainCamera == null || realCameraSocket == null)
        {
            Debug.LogWarning("[FinalEndingDirector] Main Camera 또는 Real Camera Socket이 비어 있습니다.");
            return;
        }

        // 기존 부모에서 분리 후 CameraSocket 아래로 이동
        mainCamera.transform.SetParent(realCameraSocket);

        // 여기서 0,0,0이 아니라 Inspector 값 사용
        mainCamera.transform.localPosition = firstPersonCameraLocalPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(firstPersonCameraLocalEuler);

        cameraAttachedToRealPlayer = true;
    }

    private void TurnRealPlayerInstant()
    {
        if (realPlayerRoot == null)
            return;

        Vector3 euler = realPlayerRoot.transform.eulerAngles;
        euler.y += turnLeftAngle;
        realPlayerRoot.transform.eulerAngles = euler;

        SyncRealPlayerControllerView();
    }

    private IEnumerator TurnRealPlayerAfterAttach()
    {
        if (realPlayerRoot == null)
            yield break;

        Quaternion startRot = realPlayerRoot.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, turnLeftAngle, 0f);

        float t = 0f;

        while (t < turnAfterAttachTime)
        {
            t += Time.deltaTime;

            float a = Mathf.Clamp01(t / turnAfterAttachTime);
            a = Mathf.SmoothStep(0f, 1f, a);

            realPlayerRoot.transform.rotation = Quaternion.Slerp(startRot, endRot, a);

            yield return null;
        }

        realPlayerRoot.transform.rotation = endRot;

        SyncRealPlayerControllerView();
    }

    private void SyncRealPlayerControllerView()
    {
        if (realPlayerController == null)
            return;

        float yaw = realPlayerRoot != null ? realPlayerRoot.transform.eulerAngles.y : realPlayerController.GetCurrentYaw();
        float pitch = 0f;

        realPlayerController.SetViewRotation(yaw, pitch);
    }

    private IEnumerator MoveResearcherToPlayer()
    {
        if (researcher == null || researcherArrivePoint == null)
            yield break;

        while (true)
        {
            Vector3 current = researcher.position;
            Vector3 target = researcherArrivePoint.position;

            Vector3 dir = target - current;
            dir.y = 0f;

            if (dir.magnitude <= 0.05f)
                break;

            researcher.position += dir.normalized * researcherMoveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.001f)
                researcher.forward = dir.normalized;

            yield return null;
        }
    }

    private IEnumerator ShowDialogue(string line)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = line;

        yield return new WaitForSeconds(dialogueHoldTime);
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}