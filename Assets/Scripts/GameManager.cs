using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD (TMP)")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Bricks Root (opcjonalnie)")]
    [Tooltip("Podaj np. obiekt 'Bricks' z Hierarchy. Jeœli puste, GameManager policzy Bricki w ca³ej scenie.")]
    [SerializeField] private Transform bricksRoot;

    [Header("Tuning")]
    [SerializeField] private int pointsPerHit = 10;
    [SerializeField] private int startLives = 3;

    // runtime
    public int Score { get; private set; }
    public int Lives { get; private set; }

    /// <summary> Ile ³¹cznie hitów trzeba wykonaæ, ¿eby rozwaliæ wszystkie klocki </summary>
    public int Target { get; private set; }

    /// <summary> Ile hitów zosta³o do koñca </summary>
    public int Left { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // jeœli chcesz, ¿eby GameManager prze¿ywa³ miêdzy scenami, odkomentuj:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Lives = startLives;
        Score = 0;

        RecalculateTarget();
        UpdateHud();
    }

    [ContextMenu("Recalculate Target (Debug)")]
    public void RecalculateTarget()
    {
        int totalHits = 0;

        if (bricksRoot != null)
        {
            // liczymy wszystkie Bricki bêd¹ce dzieæmi BricksRoot
            var bricks = bricksRoot.GetComponentsInChildren<Brick>(true);
            foreach (var b in bricks)
                totalHits += Mathf.Max(1, b.HitsToBreak);
        }
        else
        {
            // fallback: policz wszystkie Bricki w scenie
            var bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
            foreach (var b in bricks)
                totalHits += Mathf.Max(1, b.HitsToBreak);
        }

        Target = totalHits;
        Left = totalHits;

        UpdateHud();
    }

    /// <summary>
    /// Wo³ane przy KA¯DYM trafieniu w klocek (hit = 1).
    /// Jeœli klocek ma np. 2 ¿ycia, to dostaniesz 2 hity -> 20 pkt i -2 do Left.
    /// </summary>
    public void RegisterHit(int hits = 1)
    {
        hits = Mathf.Max(1, hits);

        Score += pointsPerHit * hits;
        Left = Mathf.Max(0, Left - hits);

        UpdateHud();

        if (Left == 0 && Target > 0)
        {
            Debug.Log("WIN! (zniszczono wszystko)");
            // TODO: WinPanel / przejœcie do EndScene
        }
    }

    public void LoseLife()
    {
        Lives = Mathf.Max(0, Lives - 1);
        UpdateHud();

        if (Lives <= 0)
        {
            Debug.Log("GAME OVER");
            // TODO: GameOverPanel / przejœcie do EndScene
        }
    }

    private void UpdateHud()
    {
        if (scoreText != null) scoreText.text = $"Score: {Score:0000}";
        if (livesText != null) livesText.text = $"Lives: {Lives}";

        // progress: wykonane / wszystkie
        int done = Mathf.Clamp(Target - Left, 0, Target);
        if (targetText != null) targetText.text = $"Target: {done}/{Target}";
    }
}
