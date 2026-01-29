using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Scenes")]
    [SerializeField] private string endSceneName = "EndGameScene";

    // runtime
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Target { get; private set; }
    public int Left { get; private set; }

    private bool levelFinished;
    private Coroutine endFlowRoutine;

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
        // na wszelki wypadek (pauza/freeze/itp.)
        Time.timeScale = 1f;

        Lives = startLives;
        Score = 0;
        levelFinished = false;

        UpdateHud();

        // Debug fallback (gdyby LevelManager nie istniał)
        if (LevelManager.Instance == null)
        {
            RecalculateTarget();
            OnLevelStarted(1);
        }
    }

    private int GetCurrentLevelNumberSafe()
    {
        return LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelNumber : 1;
    }

    [ContextMenu("Recalculate Target (Debug)")]
    public void RecalculateTarget()
    {
        int totalHits = 0;

        Brick[] bricks = (bricksRoot != null)
            ? bricksRoot.GetComponentsInChildren<Brick>(true)
            : FindObjectsByType<Brick>(FindObjectsSortMode.None);

        foreach (var b in bricks)
        {
            if (b == null) continue;
            if (b.Unbreakable) continue; // nie liczymy niezniszczalnych

            totalHits += Mathf.Max(1, b.HitsToBreak);
        }

        Target = totalHits;
        Left = totalHits;

        levelFinished = false;
        UpdateHud();

        // Jeśli level ma 0 rozwalalnych bricków -> potraktuj jako "od razu wyczyszczony"
        if (Target == 0)
        {
            levelFinished = true;

            if (endFlowRoutine != null) StopCoroutine(endFlowRoutine);
            endFlowRoutine = StartCoroutine(LevelClearFlow(GetCurrentLevelNumberSafe()));
        }
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

            int levelNumber = GetCurrentLevelNumberSafe();

            // ✅ Overlay FX (konfetti/flash/napis) w GameScene – jeśli obiekt istnieje
            if (EndGameOverlayFX.Instance != null)
                EndGameOverlayFX.Instance.ShowLevelClear(levelNumber);

            if (endFlowRoutine != null) StopCoroutine(endFlowRoutine);
            endFlowRoutine = StartCoroutine(LevelClearFlow(levelNumber));
        }
    }

    private IEnumerator LevelClearFlow(int clearedLevelNumber)
    {
        // mała pauza zanim wczytamy następny level / EndScene
        yield return new WaitForSeconds(nextLevelDelay);

        if (LevelManager.Instance != null && LevelManager.Instance.HasNextLevel())
        {
            LevelManager.Instance.LoadNextLevel();
            // LevelManager po aktywacji levela wywoła: GameManager.OnLevelStarted(...)
        }
        else
        {
            FinishGameWin();
        }

        endFlowRoutine = null;
    }

    // wołane przez LevelManager po aktywacji levela
    public void OnLevelStarted(int levelNumber)
    {
        RecalculateTarget();

        BallMove ball = FindFirstObjectByType<BallMove>();
        if (ball != null) ball.StartLevel(levelNumber);

        levelFinished = false;
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

            // ✅ Overlay FX (GAME OVER) w GameScene – jeśli obiekt istnieje
            if (EndGameOverlayFX.Instance != null)
                EndGameOverlayFX.Instance.ShowGameOver();

            if (endFlowRoutine != null) StopCoroutine(endFlowRoutine);
            endFlowRoutine = StartCoroutine(GameOverFlow());
        }
    }

    private IEnumerator GameOverFlow()
    {
        // krótka chwila na pokazanie napisu/flash (i ewentualny freeze z overlay)
        yield return new WaitForSeconds(0.35f);

        FinishGameLose();
        endFlowRoutine = null;
    }

    // ===================== END GAME =====================

    public void FinishGameWin()
    {
        EndGameData.Won = true;
        EndGameData.Score = Score;

        SceneManager.LoadScene(endSceneName, LoadSceneMode.Single);
    }

    public void FinishGameLose()
    {
        EndGameData.Won = false;
        EndGameData.Score = Score;

        SceneManager.LoadScene(endSceneName, LoadSceneMode.Single);
    }

    // ===================== HUD =====================

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
