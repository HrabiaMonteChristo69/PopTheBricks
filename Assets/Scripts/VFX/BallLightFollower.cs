using UnityEngine;
using UnityEngine.Rendering.Universal;

/*
 * BallLightFollower (Sprint 4 – 2D Lights) [FINAL]:
 * - Point Light 2D pod¹¿a za pi³k¹
 * - Opcjonalnie: w Fire Mode mo¿na podbiæ intensity/radius (na przysz³oœæ)
 *
 * Nie dotyka logiki gry – tylko VFX/œwiat³o.
 */
[DisallowMultipleComponent]
[RequireComponent(typeof(Light2D))]
public class BallLightFollower : MonoBehaviour
{
    [Header("Target (Ball)")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f);

    [Header("Smoothing")]
    [Tooltip("0 = natychmiast, >0 = wyg³adzenie (polecam 0.05–0.12).")]
    [SerializeField] private float smoothTime = 0.08f;

    // TECH: twarde wartoœci jak na screenie (dla stabilnoœci)
    [Header("HARD Defaults (jak ustawienia z Inspectora)")]
    [SerializeField] private bool enforceHardDefaults = true;
    [SerializeField] private float hardIntensity = 1.15f;
    [SerializeField] private float hardOuterRadius = 3.2f;
    [SerializeField] private float hardInnerRadius = 0.5f;

    private Light2D light2D;
    private Vector3 velocity;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();

        if (target == null)
        {
            GameObject ball = GameObject.FindGameObjectWithTag("Ball");
            if (ball != null) target = ball.transform;
        }

        if (enforceHardDefaults)
        {
            // TECH: twarde ustawienia (¿eby po zmianach w Inspectorze nie “uciek³o”)
            light2D.intensity = hardIntensity;
            light2D.pointLightOuterRadius = hardOuterRadius;
            light2D.pointLightInnerRadius = hardInnerRadius;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        if (smoothTime <= 0f)
        {
            transform.position = desired;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}
