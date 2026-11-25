using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;   // <- F로 변경
    public float interactRadius = 1.6f;

    // 플레이어만 입력으로 상호작용 하게 할지(고스트는 false)
    public bool listenToInput = true;

    void Update()
    {
        if (!listenToInput) return;

        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    // 고스트도 호출할 수 있도록 public
    public void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Interactable best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var i = h.GetComponentInParent<Interactable>();
            if (i == null) continue;

            float d = Vector3.Distance(transform.position, i.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }

        if (best != null)
            best.Interact(gameObject);
    }
}
