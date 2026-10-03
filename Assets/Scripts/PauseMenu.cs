using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;

    public static bool IsPaused { get; private set; }

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (EscapePressed())
            SetPaused(!IsPaused);
    }

    private bool EscapePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        if (_pausePanel != null) _pausePanel.SetActive(paused);
    }

    // Hook these up to your UI buttons' OnClick
    public void Resume() => SetPaused(false);

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        SetPaused(false);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Never leave the game frozen if this object gets destroyed while paused
        Time.timeScale = 1f;
        IsPaused = false;
    }
}