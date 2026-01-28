using UnityEngine;

/*
 * CameraController (Sprint 1 – Kamera wirtualna) [FINAL HARD]:
 * - ORTHO 2D
 * - HARD zoom = 12.6 (zawsze, niezale¿nie od Inspectora)
 * - X: delikatny follow pi³ki
 * - Y: bezpiecznik widocznoœci paletki (kamera nie mo¿e pójœæ za wysoko)
 * - Punch zoom + shake
 * - Shake po stracie ¿ycia 2x delikatniej (Twoja uwaga)
 */
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Target (najczêœciej Ball)")]
    [SerializeField] private Transform target;

    [Header("Paddle (do gwarancji widocznoœci)")]
    [Tooltip("Jeœli nie przypniesz, skrypt spróbuje znaleŸæ obiekt z tagiem 'Paddle'.")]
    [SerializeField] private Transform paddle;

    [Header("Follow (delikatny)")]
    [Range(0f, 1f)]
    [Tooltip("Jak mocno kamera œledzi target w osi X. Polecam 0.08 - 0.20.")]
    [SerializeField] private float followX = 0.12f;

    [Tooltip("Wyg³adzanie ruchu kamery (wiêksze = bardziej leniwa kamera).")]
    [SerializeField] private float smoothTime = 0.20f;

    [Header("Kadr bazowy")]
    [Tooltip("Sta³e przesuniêcie kadru (opcjonalne). Zwykle 0.")]
    [SerializeField] private Vector2 baseOffset = new Vector2(0f, 0f);

    [Header("Bezpiecznik paletki")]
    [Tooltip("Minimalny margines (w œwiecie) pomiêdzy paletk¹ a doln¹ krawêdzi¹ ekranu.")]
    [SerializeField] private float paddleBottomMargin = 1.15f;

    [Tooltip("Jeœli true, kamera nigdy nie pójdzie wy¿ej ni¿ pozwala widocznoœæ paletki.")]
    [SerializeField] private bool keepPaddleVisible = true;

    [Header("Zoom (HARD)")]
    [Tooltip("HARD: bazowy zoom w ortho. Twardo 12.6 (nie z Inspectora).")]
    [SerializeField] private float baseOrthoSize = 12.6f;

    [Tooltip("Punch zoom. Przy du¿ym size (12+) musi byæ wiêkszy, ¿eby by³o widaæ.")]
    [SerializeField] private float punchZoomAmount = 0.80f;

    [SerializeField] private float punchInTime = 0.08f;
    [SerializeField] private float punchOutTime = 0.14f;

    [Header("Shake")]
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private float shakeDamping = 12f;

    [Tooltip("Hard cap na shake, ¿eby kamera nigdy nie trzês³a za mocno.")]
    [SerializeField] private float maxShakeIntensity = 0.75f;

    private Camera cam;
    private Vector3 followVelocity;

    private float currentOrthoSize;

    private float shakeIntensity;
    private float shakeSeed;

    private Coroutine punchRoutine;

    private float baseCameraY;
    private Vector3 basePosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = GetComponent<Camera>();
        cam.orthographic = true;

        // TECH (HARD): niezale¿nie od tego co jest w Inspectorze, ustawiamy 12.6.
        // To rozwi¹zuje problem "wraca na 6.5".
        baseOrthoSize = 12.6f;
        cam.orthographicSize = baseOrthoSize;
        currentOrthoSize = baseOrthoSize;

        // TECH: standard 2D – kamera zwykle stoi na Z = -10.
        if (Mathf.Abs(transform.position.z) < 0.001f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }

        // TECH: baza kadru = aktualny Y kamery (ustawiasz go w scenie).
        baseCameraY = transform.position.y;

        // Auto-find paddle po tagu (jeœli nie przypiêta).
        if (paddle == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Paddle");
            if (p != null) paddle = p.transform;
        }

        shakeSeed = Random.Range(0f, 9999f);
    }

    private void LateUpdate()
    {
        UpdateFollowAndFraming();
        UpdateShake();
        ApplyTransform();
        cam.orthographicSize = currentOrthoSize;
    }

    private void UpdateFollowAndFraming()
    {
        float desiredX = transform.position.x + baseOffset.x;

        if (target != null)
        {
            desiredX = Mathf.Lerp(transform.position.x, target.position.x, followX) + baseOffset.x;
        }

        float desiredY = baseCameraY + baseOffset.y;

        // BEZPIECZNIK: paletka ma byæ zawsze na ekranie (kamera nie mo¿e pójœæ za wysoko).
        if (keepPaddleVisible && paddle != null)
        {
            // camY <= paddleY + orthoSize - margin
            float maxAllowedY = paddle.position.y + cam.orthographicSize - paddleBottomMargin;
            desiredY = Mathf.Min(desiredY, maxAllowedY);
        }

        Vector3 desired = new Vector3(desiredX, desiredY, transform.position.z);
        basePosition = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, smoothTime);
    }

    private void UpdateShake()
    {
        if (shakeIntensity <= 0f) return;

        float decay = shakeDamping * Time.deltaTime;
        shakeIntensity = Mathf.Max(0f, shakeIntensity - decay);
    }

    private void ApplyTransform()
    {
        Vector2 shakeOffset = Vector2.zero;

        if (shakeIntensity > 0f)
        {
            float t = Time.time * shakeFrequency + shakeSeed;
            float nx = Mathf.PerlinNoise(t, 0.1f) - 0.5f;
            float ny = Mathf.PerlinNoise(0.1f, t) - 0.5f;
            shakeOffset = new Vector2(nx, ny) * shakeIntensity;
        }

        transform.position = basePosition + new Vector3(shakeOffset.x, shakeOffset.y, 0f);
    }

    // ===================== Public API =====================

    public void SetTarget(Transform newTarget) => target = newTarget;

    /*
     * Shake presets:
     * - BIG jest 2x delikatniej ni¿ wczeœniej (Twoja uwaga).
     */
    public void ShakeSmall() => AddShake(0.12f);
    public void ShakeMedium() => AddShake(0.22f);
    public void ShakeBig() => AddShake(0.28f); // by³o ok. 0.55 -> teraz ~2x mniej

    public void AddShake(float amount)
    {
        shakeIntensity = Mathf.Clamp(shakeIntensity + amount, 0f, maxShakeIntensity);
    }

    public void PunchZoom(float extraAmount = 0f)
    {
        float amount = Mathf.Max(0f, punchZoomAmount + extraAmount);

        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(PunchZoomRoutine(amount));
    }

    private System.Collections.IEnumerator PunchZoomRoutine(float amount)
    {
        float from = baseOrthoSize;
        float to = Mathf.Max(0.1f, baseOrthoSize - amount);

        float t = 0f;
        while (t < punchInTime)
        {
            t += Time.deltaTime;
            float p = punchInTime <= 0f ? 1f : Mathf.Clamp01(t / punchInTime);
            currentOrthoSize = Mathf.Lerp(from, to, p);
            yield return null;
        }

        t = 0f;
        while (t < punchOutTime)
        {
            t += Time.deltaTime;
            float p = punchOutTime <= 0f ? 1f : Mathf.Clamp01(t / punchOutTime);
            currentOrthoSize = Mathf.Lerp(to, from, p);
            yield return null;
        }

        currentOrthoSize = baseOrthoSize;
        punchRoutine = null;
    }
}
