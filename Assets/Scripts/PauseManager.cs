using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene names")]
    [SerializeField] private string menuSceneName = "MenuScene";

    private bool isPaused;

    private void Awake()
    {
        // Bezpiecznik: jeœli nie podpinasz w Inspectorze, spróbuj znaleŸæ po nazwie
        if (pausePanel == null)
        {
            GameObject found = GameObject.Find("Pause_Panel");
            if (found != null) pausePanel = found;
        }
    }

    private void Start()
    {
        // Start gry zawsze wznawia czas i chowa panel
        ResumeInternal();
    }

    private void Update()
    {
        if (WasPausePressedThisFrame())
        {
            TogglePause();
        }
    }

    private bool WasPausePressedThisFrame()
    {
        bool pressed = Input.GetKeyDown(KeyCode.Escape);

#if ENABLE_INPUT_SYSTEM
        if (!pressed && Keyboard.current != null)
            pressed = Keyboard.current.escapeKey.wasPressedThisFrame;
#endif

        return pressed;
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        ResumeInternal();
    }

    private void ResumeInternal()
    {
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // ====== BUTTONS ======

    public void GoToMenu()
    {
        ResumeInternal();
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }

    public void RestartLevel()
    {
        ResumeInternal();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
