using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private Transform levelsRoot; // LevelsRoot z Hierarchy
    [SerializeField] private int startLevelIndex = 0;

    private GameObject[] levels;
    private int current;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (levelsRoot == null)
        {
            Debug.LogError("LevelManager: levelsRoot nie jest ustawiony!");
            return;
        }

        int n = levelsRoot.childCount;
        if (n <= 0)
        {
            Debug.LogError("LevelManager: levelsRoot nie ma dzieci (leveli)!");
            return;
        }

        levels = new GameObject[n];
        for (int i = 0; i < n; i++)
            levels[i] = levelsRoot.GetChild(i).gameObject;

        // Wyłącz wszystkie
        for (int i = 0; i < n; i++)
            levels[i].SetActive(false);

        // Włącz startowy
        current = Mathf.Clamp(startLevelIndex, 0, n - 1);
        ActivateLevel(current);
    }

    /// <summary>Czy istnieje kolejny level.</summary>
    public bool HasNextLevel()
    {
        if (levels == null) return false;
        return (current + 1) < levels.Length;
    }

    public void LoadNextLevel()
    {
        if (levels == null || levels.Length == 0) return;

        // Wyłącz aktualny
        if (current >= 0 && current < levels.Length)
            levels[current].SetActive(false);

        current++;

        // Koniec gry (brak kolejnych leveli)
        if (current >= levels.Length)
        {
            Debug.Log("KONIEC GRY / BRAK KOLEJNYCH LEVELI");
            if (GameManager.Instance != null) GameManager.Instance.FinishGameWin();
            return;
        }

        ActivateLevel(current);
    }

    private void ActivateLevel(int index)
    {
        if (index < 0 || index >= levels.Length) return;

        levels[index].SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBricksRoot(levels[index].transform);
            GameManager.Instance.OnLevelStarted(index + 1); // ✅ LVL X + reset piłki + target
        }
        else
        {
            Debug.LogWarning("LevelManager: Brak GameManager.Instance!");
        }
    }

    public int CurrentLevelNumber => current + 1;
}
