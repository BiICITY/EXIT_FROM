using UnityEngine;

public class FallDeath : MonoBehaviour
{
    public float killY = -10f;
    PlayerDeathController death;

    void Awake()
    {
        death = GetComponent<PlayerDeathController>();
    }

    void Update()
    {
        if (transform.position.y < killY)
        {
            if (death != null) death.Die("Fall");
        }
    }
}
