using UnityEngine;
using TMPro;

public class BallMove : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;

    [Header("Start po czasie")]
    [SerializeField] private float launchDelay = 3f;
    [SerializeField] private float startTextDuration = 0.7f;
    public TextMeshProUGUI countdownText;

    [Header("Referencje")]
    [SerializeField] private Transform paddle;        // przypnij w Inspectorze albo ustaw tag Paddle
    [SerializeField] private float loseMargin = 0.2f; // zapas pod paletką

    [Header("Spawn nad paletką")]
    [SerializeField] private float spawnOffsetY = 0.2f; // dodatkowy dystans nad paletką

    [Header("Odbicie od paletki")]
    [Range(0f, 85f)]
    public float maxBounceAngle = 60f;

    [Header("Tekst levela (po przejściu)")]
    [SerializeField] private float levelBannerDuration = 0.9f;

    [Header("Anti-stuck (żeby nie jechała po suficie)")]
    [SerializeField] private float minYAbs = 0.25f;       // minimalna "pionowość" kierunku
    [SerializeField] private float nudgeStrength = 0.06f; // delikatny kopniak (opcjonalnie)

    private Rigidbody2D rb;

    private float timer;
    private bool launched = false;
    private bool showingStart = false;

    private float ballHalfHeight = 0.1f;
    private float paddleHalfHeight = 0.2f;
    private bool losingNow = false;

    private bool showingLevelBanner = false;
    private float levelBannerTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (paddle == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Paddle");
            if (p != null) paddle = p.transform;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) ballHalfHeight = col.bounds.extents.y;

        if (paddle != null)
        {
            var pcol = paddle.GetComponent<Collider2D>();
            if (pcol != null) paddleHalfHeight = pcol.bounds.extents.y;
        }
    }

    void Start()
    {
        // Start gry = Level 1 (jeśli LevelManager wywoła StartLevel, to i tak nadpisze)
        StartLevel(1);
    }

    void Update()
    {
        if (paddle == null) return;

        // Przed startem: piłka przyklejona do paletki
        if (!launched)
        {
            SnapToPaddle();

            // 1) Najpierw pokazujemy napis LVL X
            if (showingLevelBanner)
            {
                levelBannerTimer -= Time.deltaTime;
                if (levelBannerTimer <= 0f)
                {
                    showingLevelBanner = false;
                    if (countdownText != null) countdownText.text = "";
                }
                return; // nie schodzimy jeszcze z timera
            }

            // 2) Potem normalne odliczanie
            timer -= Time.deltaTime;

            if (countdownText != null && !showingStart)
            {
                int sec = Mathf.CeilToInt(Mathf.Max(0f, timer));
                countdownText.text = sec > 0 ? sec.ToString() : "";
            }

            if (timer <= 0f && !launched)
            {
                LaunchBall();
                launched = true;

                if (countdownText != null)
                {
                    countdownText.text = "START";
                    showingStart = true;
                    Invoke(nameof(ClearCountdown), startTextDuration);
                }
            }
        }

        // Przegrana gdy piłka spadnie poniżej paletki
        float loseLineY = paddle.position.y - paddleHalfHeight - ballHalfHeight - loseMargin;
        if (!losingNow && transform.position.y < loseLineY)
        {
            losingNow = true;
            OnBallLost();
        }
    }

    private void OnBallLost()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.Lives > 0)
            {
                // po stracie życia wracamy do paletki i odliczamy od nowa
                ResetToPaddle();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        losingNow = false;
    }

    public void StartLevel(int levelNumber)
    {
        ResetToPaddle();

        showingLevelBanner = true;
        levelBannerTimer = levelBannerDuration;

        if (countdownText != null)
            countdownText.text = $"LVL {levelNumber}";
    }

    public void ResetToPaddle()
    {
        CancelInvoke();

        launched = false;
        showingStart = false;

        timer = launchDelay;
        rb.linearVelocity = Vector2.zero;

        if (countdownText != null) countdownText.text = "";
        SnapToPaddle();
    }

    private void SnapToPaddle()
    {
        if (paddle == null) return;

        float y = paddle.position.y + paddleHalfHeight + ballHalfHeight + spawnOffsetY;
        transform.position = new Vector3(paddle.position.x, y, transform.position.z);
    }

    void ClearCountdown()
    {
        if (countdownText != null) countdownText.text = "";
    }

    void LaunchBall()
    {
        // START prosto do góry
        Vector2 direction = Vector2.up;
        rb.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Odbicie od paletki
        if (collision.collider.CompareTag("Paddle"))
        {
            float hitX = collision.GetContact(0).point.x;
            float paddleX = collision.transform.position.x;
            float half = collision.collider.bounds.extents.x;

            float t = Mathf.Clamp((hitX - paddleX) / half, -1f, 1f);
            float angle = t * maxBounceAngle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
            rb.linearVelocity = dir * speed;
            return;
        }

        // Stała prędkość po innych zderzeniach
        Vector2 dir2 = rb.linearVelocity.normalized;
        if (dir2.sqrMagnitude < 0.0001f) dir2 = Vector2.up;

        // Anti-stuck: jeżeli leci prawie poziomo, wymuś minimalne |Y|
        if (Mathf.Abs(dir2.y) < minYAbs)
        {
            float signY = (dir2.y >= 0f) ? 1f : -1f;
            dir2.y = signY * minYAbs;

            float signX = (dir2.x >= 0f) ? 1f : -1f;
            dir2.x = signX * Mathf.Sqrt(Mathf.Max(0f, 1f - dir2.y * dir2.y));

            // opcjonalny mini “kop” żeby nie wpadało w identyczne odbicia
            dir2.x += Random.Range(-nudgeStrength, nudgeStrength);
            dir2 = dir2.normalized;
        }

        rb.linearVelocity = dir2 * speed;
    }
}
