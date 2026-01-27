using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD (TMP)")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Bricks Root")]
    [Tooltip("Ustawiane przez LevelManager na aktualny Level.")]
    [SerializeField] private Transform bricksRoot;

    [Header("Tuning")]
    [SerializeField] private int pointsPerHit = 10;
    [SerializeField] private int startLives = 3;

    [Header("Level flow")]
    [SerializeField] private float nextLevelDelay = 1.0f;

    // runtime
    public int Score { get; private set; }
    public int Lives { get; private set; }

    public int Target { get; private set; }
    public int Left { get; private set; }

    private bool levelFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Lives = startLives;
        Score = 0;
        levelFinished = false;

        UpdateHud();

        // Jeśli LevelManager nie istnieje (debug), policz target z aktualnego bricksRoot
        if (LevelManager.Instance == null)
        {
            RecalculateTarget();
            OnLevelStarted(1);
        }
    }

    [ContextMenu("Recalculate Target (Debug)")]
    public void RecalculateTarget()
    {
        int totalHits = 0;

        Brick[] bricks;

        // Najważniejsze: liczymy tylko bricki z aktywnego levela (bricksRoot ustawiany przez LevelManager)
        if (bricksRoot != null)
            bricks = bricksRoot.GetComponentsInChildren<Brick>(true);
        else
            bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);

        foreach (var b in bricks)
        {
            if (b == null) continue;

            // ✅ nie liczymy niezniszczalnych
            if (b.Unbreakable) continue;

            // hitsToBreak: 1..n
            totalHits += Mathf.Max(1, b.HitsToBreak);
        }

        Target = totalHits;
        Left = totalHits;

        levelFinished = false;
        UpdateHud();
    }

    public void RegisterHit(int hits = 1)
    {
        if (levelFinished) return;

        hits = Mathf.Max(1, hits);

        Score += pointsPerHit * hits;
        Left = Mathf.Max(0, Left - hits);

        UpdateHud();

        if (Left == 0 && Target > 0)
        {
            levelFinished = true;
            Debug.Log("LEVEL CLEAR!");
            Invoke(nameof(NextLevel), nextLevelDelay);
        }
    }

    private void NextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogWarning("Brak LevelManager w scenie!");
        }
    }

    // ✅ Wołane przez LevelManager po aktywacji levela
    public void OnLevelStarted(int levelNumber)
    {
        // przelicz target (na wypadek gdyby ktoś zapomniał)
        RecalculateTarget();

        // reset piłki + pokaz LVL X
        BallMove ball = FindFirstObjectByType<BallMove>();
        if (ball != null) ball.StartLevel(levelNumber);
        else Debug.LogWarning("GameManager: Nie znaleziono BallMove w scenie!");

        UpdateHud();
    }

    public void LoseLife()
    {
        if (levelFinished) return;

        Lives = Mathf.Max(0, Lives - 1);
        UpdateHud();

        if (Lives <= 0)
        {
            levelFinished = true;
            Debug.Log("GAME OVER");
            // TODO: panel / restart / menu
        }
    }

    private void UpdateHud()
    {
        if (scoreText != null) scoreText.text = $"Score: {Score:0000}";
        if (livesText != null) livesText.text = $"Lives: {Lives}";

        int done = Mathf.Clamp(Target - Left, 0, Target);
        if (targetText != null) targetText.text = $"Target: {done}/{Target}";
    }

    public void RefreshHud() => UpdateHud();

    public void SetBricksRoot(Transform newRoot)
    {
        bricksRoot = newRoot;
    }
}
