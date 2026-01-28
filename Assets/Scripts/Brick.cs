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

    // TECH (Sprint 2 – Animacje): opcjonalny komponent VFX na prefabie ceg³y.
    private BrickVfx brickVfx;

    public bool Unbreakable => unbreakable;

    // Dla unbreakable: 0 (¿eby GameManager móg³ to pomin¹æ w target)
    public int HitsToBreak => Unbreakable ? 0 : Mathf.Max(1, hitsToBreak);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseSprite = sr != null ? sr.sprite : null;

        // TECH: jeœli jest BrickVfx na tym samym GO, u¿yjemy go.
        brickVfx = GetComponent<BrickVfx>();

        ResetState();
    }

    private void OnEnable()
    {
        // wa¿ne przy levelach (w³¹cz/wy³¹cz)
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (baseSprite == null && sr != null) baseSprite = sr.sprite;

        // TECH: ponowne pobranie (gdy prefab/level by³ reloaded)
        if (brickVfx == null) brickVfx = GetComponent<BrickVfx>();

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

        // TECH (Sprint 1 – Kamera):
        // - trafienie w niezniszczaln¹ ceg³ê = mocniejszy feedback (œredni shake),
        //   bo "twardy metal" i odbicie powinno byæ odczuwalne.
        if (Unbreakable)
        {
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeMedium();
            }
            return;
        }

        // TECH (Sprint 2 – Fire Mode): ka¿de trafienie w niszczaln¹ ceg³ê liczy siê do 6 hitów.
        // Minimalnie-inwazyjnie: odwo³ujemy siê do komponentu na pi³ce, jeœli istnieje.
        var fire = collision.collider.GetComponent<FireModeOnBall>();
        if (fire != null) fire.NotifyBreakableBrickHit();

        // klocek dostaje dmg
        hp--;

        // TECH (Sprint 2 – Animacje): hit squash (jeœli mamy komponent)
        if (brickVfx != null) brickVfx.PlayHit();

        // TECH (Sprint 1 – Kamera):
        // - trafienie w zniszczaln¹ ceg³ê = ma³y shake (feedback), ¿eby gra by³a "miêsista".
        // - przy zniszczeniu damy te¿ punch zoom (krótkie przybli¿enie).
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeSmall();
        }

        // 1 hit = 10 pkt + -1 do targetu
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterHit(1);

        if (hp <= 0)
        {
            // TECH (Sprint 1 – Kamera):
            // Zniszczenie ceg³y = jeszcze lepszy "impact": lekki punch zoom.
            if (CameraController.Instance != null)
            {
                CameraController.Instance.PunchZoom(0.10f);
            }

            // TECH (Sprint 2 – Animacje): zamiast natychmiastowego Destroy,
            // robimy pop+rot+fade, a potem niszczymy obiekt.
            if (brickVfx != null)
            {
                brickVfx.PlayBreakAndDestroy();
            }
            else
            {
                Destroy(gameObject);
            }

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
