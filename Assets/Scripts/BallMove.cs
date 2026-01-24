using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Odbicie od paletki")]
    [Range(0f, 85f)]
    public float maxBounceAngle = 60f; // im więcej, tym bardziej "w bok" może polecieć

    private Rigidbody2D rb;
    private float timer;
    private bool launched = false;
    private float ballHalfHeight = 0.1f;
    private bool showingStart = false;

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
    }

    void Start()
    {
        timer = launchDelay;
        rb.linearVelocity = Vector2.zero; // Unity 6
        if (countdownText != null) countdownText.text = "";
    }

    void Update()
    {
        // Odliczanie + start
        if (!launched)
        {
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
        if (paddle != null)
        {
            float loseLineY = paddle.position.y - ballHalfHeight - loseMargin;
            if (transform.position.y < loseLineY)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void ClearCountdown()
    {
        if (countdownText != null) countdownText.text = "";
    }

    void LaunchBall()
    {
        // Start w stronę paletki (w dół) z lekkim odchyleniem
        Vector2 direction = new Vector2(0.2f, -1f).normalized;
        rb.linearVelocity = direction * speed; // Unity 6
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Arkanoidowe odbicie od paletki
        if (collision.collider.CompareTag("Paddle"))
        {
            // punkt uderzenia na paletce (-1 lewo, +1 prawo)
            float hitX = collision.GetContact(0).point.x;
            float paddleX = collision.transform.position.x;
            float half = collision.collider.bounds.extents.x;

            float t = Mathf.Clamp((hitX - paddleX) / half, -1f, 1f);

            // kąt względem "do góry"
            float angle = t * maxBounceAngle * Mathf.Deg2Rad;

            // sin -> X, cos -> Y (Y zawsze dodatnie, czyli odbija w górę)
            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
            rb.linearVelocity = dir * speed;
            return;
        }

        // Utrzymuj stałą prędkość po innych zderzeniach (ściany/klocki)
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) v = Vector2.up;
        rb.linearVelocity = v.normalized * speed;
    }
}
