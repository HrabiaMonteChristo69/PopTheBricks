using System.Collections;
using UnityEngine;

/*
 * BrickVfx (Sprint 2 – Animacje cegie³) [FINAL]:
 * - Brick hit (niezniszczony): krótki squash/punch (proceduralnie, bez Animatora)
 * - Brick break (zniszczenie): pop (scale 1 -> 1.15 -> 0) + ma³y obrót + fade-out
 *
 * TECH:
 * - Nie zmienia logiki Brick / GameManager (punkty/target) — tylko VFX.
 * - Brick.cs wo³a tu PlayHit() / PlayBreakAndDestroy() jeœli komponent jest na prefabie.
 */
[DisallowMultipleComponent]
public class BrickVfx : MonoBehaviour
{
    [Header("Hit (squash)")]
    [SerializeField] private float hitScaleX = 1.62f;
    [SerializeField] private float hitScaleY = 1.86f;
    [SerializeField] private float hitInTime = 0.25f;
    [SerializeField] private float hitOutTime = 0.30f;

    [Header("Break (pop + rot + fade)")]
    [SerializeField] private float breakPopScale = 1.50f;
    [SerializeField] private float breakPopTime = 0.50f;
    [SerializeField] private float breakShrinkTime = 0.45f;
    [SerializeField] private float breakRotateDeg = 60f;

    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 baseScale;
    private Quaternion baseRot;

    private Coroutine hitRoutine;
    private Coroutine breakRoutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        baseScale = transform.localScale;
        baseRot = transform.localRotation;
    }

    private void OnEnable()
    {
        // TECH: levele s¹ w³¹czane/wy³¹czane — wracamy do bazowego stanu.
        transform.localScale = baseScale;
        transform.localRotation = baseRot;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    // ===================== API =====================

    public void PlayHit()
    {
        if (!isActiveAndEnabled) return;
        if (breakRoutine != null) return; // ju¿ siê "rozpada" — nie ma sensu squashowaæ

        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitRoutine());
    }

    /// <summary>
    /// Zamiast Destroy() od razu — ceg³a robi animacjê i sama siê niszczy na koñcu.
    /// </summary>
    public void PlayBreakAndDestroy()
    {
        if (!isActiveAndEnabled)
        {
            Destroy(gameObject);
            return;
        }

        if (breakRoutine != null) return;

        // TECH: ¿eby pi³ka nie dobija³a obiektu w trakcie animacji
        if (col != null) col.enabled = false;

        if (hitRoutine != null) StopCoroutine(hitRoutine);
        breakRoutine = StartCoroutine(BreakRoutine());
    }

    // ===================== Routines =====================

    private IEnumerator HitRoutine()
    {
        Vector3 from = baseScale;
        Vector3 to = new Vector3(baseScale.x * hitScaleX, baseScale.y * hitScaleY, baseScale.z);

        // IN
        float t = 0f;
        while (t < hitInTime)
        {
            t += Time.deltaTime;
            float p = hitInTime <= 0f ? 1f : Mathf.Clamp01(t / hitInTime);
            transform.localScale = Vector3.Lerp(from, to, p);
            yield return null;
        }

        // OUT
        t = 0f;
        while (t < hitOutTime)
        {
            t += Time.deltaTime;
            float p = hitOutTime <= 0f ? 1f : Mathf.Clamp01(t / hitOutTime);
            transform.localScale = Vector3.Lerp(to, from, p);
            yield return null;
        }

        transform.localScale = baseScale;
        hitRoutine = null;
    }

    private IEnumerator BreakRoutine()
    {
        // Pop + lekki obrót (losowo lewo/prawo)
        float dir = Random.value < 0.5f ? -1f : 1f;
        Quaternion rotFrom = baseRot;
        Quaternion rotTo = Quaternion.Euler(0f, 0f, breakRotateDeg * dir) * baseRot;

        Vector3 scaleFrom = baseScale;
        Vector3 scalePop = baseScale * breakPopScale;

        // 1) POP
        float t = 0f;
        while (t < breakPopTime)
        {
            t += Time.deltaTime;
            float p = breakPopTime <= 0f ? 1f : Mathf.Clamp01(t / breakPopTime);
            transform.localScale = Vector3.Lerp(scaleFrom, scalePop, p);
            transform.localRotation = Quaternion.Slerp(rotFrom, rotTo, p);
            yield return null;
        }

        // 2) SHRINK + FADE
        float startAlpha = 1f;
        if (sr != null) startAlpha = sr.color.a;

        t = 0f;
        while (t < breakShrinkTime)
        {
            t += Time.deltaTime;
            float p = breakShrinkTime <= 0f ? 1f : Mathf.Clamp01(t / breakShrinkTime);

            transform.localScale = Vector3.Lerp(scalePop, Vector3.zero, p);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(startAlpha, 0f, p);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
