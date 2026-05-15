using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;
    public CanvasFollowHead menuFollow;
    public SportsVRGameManager gameManager;

    [Header("Keyboard Testing")]
    public KeyCode keyboardPauseKey = KeyCode.Escape;

    [Header("Pause Behavior")]
    public bool freezeTimeWhenPaused = true;

    bool isPaused;

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(keyboardPauseKey))
            TogglePause();
    }

    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    public void ContinueGame()
    {
        SetPaused(false);
    }

    public void RestartSession()
    {
        SetPaused(false);
        if (gameManager != null) gameManager.RestartCurrentSession();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToLobby()
    {
        SetPaused(false);
        if (gameManager != null) gameManager.ShowLobby();
    }

    public void QuitGame()
    {
        if (gameManager != null) gameManager.QuitGame();
        else Application.Quit();
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (isPaused && menuFollow != null)
            menuFollow.SnapInFrontOfPlayer();

        if (freezeTimeWhenPaused)
            Time.timeScale = isPaused ? 0f : 1f;
    }
}