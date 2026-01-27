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

        // Pobierz dzieci LevelsRoot jako levele
        int n = levelsRoot.childCount;
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

    public void LoadNextLevel()
    {
        if (levels == null || levels.Length == 0) return;

        // Wyłącz aktualny
        levels[current].SetActive(false);

        current++;
        if (current >= levels.Length)
        {
            Debug.Log("KONIEC GRY / BRAK KOLEJNYCH LEVELI");
            return;
        }

        // Włącz następny
        ActivateLevel(current);
    }

    private void ActivateLevel(int index)
    {
        if (index < 0 || index >= levels.Length) return;

        levels[index].SetActive(true);

        // Ustaw root bricków na aktualny level
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBricksRoot(levels[index].transform);
            GameManager.Instance.RecalculateTarget();
            GameManager.Instance.OnLevelStarted(index + 1); // ✅ pokazuje napis LVL X + reset piłki
        }
        else
        {
            Debug.LogWarning("LevelManager: Brak GameManager.Instance!");
        }
    }

    public int CurrentLevelNumber => current + 1;
}
