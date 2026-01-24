using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame() => SceneManager.LoadScene("GameScene");
    public void LoadMenu() => SceneManager.LoadScene("MenuScene");
    public void LoadEnd() => SceneManager.LoadScene("EndGameScene");

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QuitGame()");
    }
}
