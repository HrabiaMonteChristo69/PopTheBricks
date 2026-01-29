using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    [Header("Panels (EndGameScene)")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Texts (optional)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Scene names")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        Time.timeScale = 1f;

        // auto-find jeœli zapomnia³eœ przypi¹æ
        if (winPanel == null)
        {
            var go = GameObject.Find("Panel_Win");
            if (go != null) winPanel = go;
        }
        if (losePanel == null)
        {
            var go = GameObject.Find("Panel_Lose");
            if (go != null) losePanel = go;
        }

        bool won = EndGameData.Won;

        if (winPanel != null) winPanel.SetActive(won);
        if (losePanel != null) losePanel.SetActive(!won);

        // Ustaw tytu³y w panelach (masz po dwa TitleText – w ka¿dym panelu osobno)
        if (winPanel != null)
        {
            var t = winPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (t != null) t.text = "YOU WIN";
        }
        if (losePanel != null)
        {
            var t = losePanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (t != null) t.text = "GAME OVER";
        }

        if (scoreText != null)
            scoreText.text = $"Score: {EndGameData.Score:0000}";
    }

    // Pod to podepniesz przyciski:
    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
}
