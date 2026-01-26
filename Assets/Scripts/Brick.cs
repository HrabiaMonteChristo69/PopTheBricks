using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Brick : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private bool unbreakable = false;

    [Tooltip("Ile razy trzeba trafiæ, ¿eby zniszczyæ (dla unbreakable ignorowane).")]
    [SerializeField, Min(1)] private int hitsToBreak = 1;

    [Header("Visual states (optional)")]
    [Tooltip("Jeœli ustawisz, sprite bêdzie siê zmienia³ po trafieniach (0 = stan pocz¹tkowy).")]
    [SerializeField] private Sprite[] damageSprites;

    [Tooltip("Jeœli nie u¿ywasz sprite'ów, mo¿esz ustawiæ kolory stanów (0 = stan pocz¹tkowy).")]
    [SerializeField] private Color[] damageColors;

    [Header("Hit feedback")]
    [SerializeField] private float flashTime = 0.06f;
    [SerializeField] private Color flashColor = Color.white;

    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float punchTime = 0.08f;

    private int hitsLeft;
    private SpriteRenderer sr;
    private Color baseColor;
    private Vector3 baseScale;

    // Dla GameManagera: target ma liczyæ tylko niszczalne
    public int HitsToBreak => unbreakable ? 0 : Mathf.Max(1, hitsToBreak);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        baseScale = transform.localScale;

        hitsLeft = unbreakable ? int.MaxValue : Mathf.Max(1, hitsToBreak);
        ApplyDamageVisual();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Upewnij siê, ¿e pi³ka ma tag "Ball"
        if (!collision.collider.CompareTag("Ball"))
            return;

        if (unbreakable)
        {
            StartCoroutine(HitFeedback());
            return;
        }

        hitsLeft--;

        // ka¿dy hit = 10 pkt i 1 do Target/Left
        GameManager.Instance?.RegisterHit(1);

        if (hitsLeft <= 0)
        {
            Destroy(gameObject);
            return;
        }

        ApplyDamageVisual();
        StartCoroutine(HitFeedback());
    }

    private void ApplyDamageVisual()
    {
        if (unbreakable) return;

        int damageIndex = Mathf.Clamp(hitsToBreak - hitsLeft, 0,
            Mathf.Max(0, Mathf.Max(damageSprites.Length, damageColors.Length) - 1));

        // Sprites maj¹ pierwszeñstwo
        if (damageSprites != null && damageSprites.Length > 0)
        {
            int idx = Mathf.Clamp(damageIndex, 0, damageSprites.Length - 1);
            sr.sprite = damageSprites[idx];
            baseColor = sr.color; // nie zmieniamy koloru, tylko sprite
        }
        else if (damageColors != null && damageColors.Length > 0)
        {
            int idx = Mathf.Clamp(damageIndex, 0, damageColors.Length - 1);
            sr.color = damageColors[idx];
            baseColor = sr.color;
        }
    }

    private IEnumerator HitFeedback()
    {
        // Flash
        Color prev = sr.color;
        sr.color = flashColor;

        // Punch scale
        Vector3 punch = baseScale * (1f + punchScale);
        transform.localScale = punch;

        yield return new WaitForSeconds(flashTime);

        sr.color = prev;

        // szybki powrót skali
        float t = 0f;
        while (t < punchTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, t / punchTime);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}
