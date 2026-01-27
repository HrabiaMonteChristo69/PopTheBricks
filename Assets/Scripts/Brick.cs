using UnityEngine;

public class Brick : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private bool unbreakable = false;

    [Header("Health")]
    [Min(1)]
    [SerializeField] private int hitsToBreak = 1;

    [Header("Visual damage (opcjonalne)")]
    [Tooltip("Sprite po 1 trafieniu, po 2 trafieniu, itd. (dla Brick_3 ustaw 2 sprity).")]
    [SerializeField] private Sprite[] damageSprites;

    private int hp;
    private SpriteRenderer sr;
    private Sprite baseSprite;

    public bool Unbreakable => unbreakable;

    // Dla unbreakable: 0 (¿eby GameManager móg³ to pomin¹æ w target)
    public int HitsToBreak => Unbreakable ? 0 : Mathf.Max(1, hitsToBreak);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseSprite = sr != null ? sr.sprite : null;

        ResetState();
    }

    private void OnEnable()
    {
        // wa¿ne przy levelach (w³¹cz/wy³¹cz)
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (baseSprite == null && sr != null) baseSprite = sr.sprite;

        ResetState();
    }

    private void ResetState()
    {
        if (Unbreakable)
        {
            hp = int.MaxValue;
        }
        else
        {
            hp = Mathf.Max(1, hitsToBreak);
        }

        if (sr != null && baseSprite != null)
            sr.sprite = baseSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // opcjonalnie: ogranicz do pi³ki (polecam)
        if (!collision.collider.CompareTag("Ball")) return;

        if (Unbreakable) return;

        // klocek dostaje dmg
        hp--;

        // 1 hit = 10 pkt + -1 do targetu
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterHit(1);

        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (sr == null) return;

        int hitsTaken = hitsToBreak - hp; // po 1 trafieniu = 1
        int idx = hitsTaken - 1;

        if (damageSprites != null && idx >= 0 && idx < damageSprites.Length && damageSprites[idx] != null)
            sr.sprite = damageSprites[idx];
        else
            sr.sprite = baseSprite; // fallback
    }
}
