using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Vector3 baseLocalPos;

    float timeLeft;
    float duration;
    float amplitude;
    float frequency;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        baseLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        baseLocalPos = transform.localPosition;
    }

    public void Shake(float amp, float dur, float freq = 30f)
    {
        // 여러 번 호출되면 더 강한 값/더 긴 값으로 갱신
        amplitude = Mathf.Max(amplitude, amp);
        duration = Mathf.Max(duration, dur);
        frequency = Mathf.Max(frequency, freq);
        timeLeft = duration;
    }

    void LateUpdate()
    {
        if (timeLeft <= 0f)
        {
            transform.localPosition = baseLocalPos;
            amplitude = 0f;
            duration = 0f;
            return;
        }

        timeLeft -= Time.deltaTime;

        float t = Time.unscaledTime * frequency;

        // Perlin 기반 부드러운 흔들림
        float nx = (Mathf.PerlinNoise(t, 0.1f) * 2f - 1f);
        float ny = (Mathf.PerlinNoise(0.1f, t) * 2f - 1f);

        float k = Mathf.Clamp01(timeLeft / Mathf.Max(0.0001f, duration)); // 끝으로 갈수록 약해짐
        Vector3 offset = new Vector3(nx, ny, 0f) * (amplitude * k);

        transform.localPosition = baseLocalPos + offset;
    }
}
