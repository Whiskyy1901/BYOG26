using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    private const string HighscoreKey = "Highscore";

    [Header("References")]
    [Tooltip("The game over panel. Keep this script on an object that stays active (e.g. the Canvas).")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _wavesText;
    [SerializeField] private TextMeshProUGUI _highscoreText;
    [SerializeField] private TextMeshProUGUI _newBestText;

    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _quitButton;

    [Header("New best flash")]
    [SerializeField] private float _flashInterval = 0.4f;
    
    [Header("Hide when game over shows")]
    [SerializeField] private GameObject[] _hideOnShow;

    private WaveManager _waveManager;
    private bool _isShowing;

    private void Awake()
    {
        _waveManager = FindAnyObjectByType<WaveManager>();

        _restartButton.onClick.AddListener(Restart);
        _quitButton.onClick.AddListener(Quit);

        _panel.SetActive(false);
        if (_newBestText != null) _newBestText.gameObject.SetActive(false);
    }

    // Call this from wherever the player dies.
    public void Show()
    {
        if (_isShowing) return;
        _isShowing = true;

        foreach (var item in _hideOnShow)
            if (item != null) item.SetActive(false);
        
        int waves = _waveManager != null ? _waveManager.WaveNumber : 0;
        int best = PlayerPrefs.GetInt(HighscoreKey, 0);
        bool newBest = waves > best;

        if (newBest)
        {
            best = waves;
            PlayerPrefs.SetInt(HighscoreKey, best);
            PlayerPrefs.Save();
        }

        _wavesText.text = "WAVES SURVIVED: " + waves;
        _highscoreText.text = "HIGHSCORE: " + best;

        
        // Freeze the game and stop new waves from spawning.
        //Time.timeScale = 0f;
        if (_waveManager != null) _waveManager.enabled = false;

        _panel.SetActive(true);

        // Pre-select Restart so keyboard/gamepad navigation works through the EventSystem.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_restartButton.gameObject);

        if (newBest && _newBestText != null)
            StartCoroutine(FlashNewBest());
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private IEnumerator FlashNewBest()
    {
        var wait = new WaitForSecondsRealtime(_flashInterval); // realtime, since timeScale is 0
        GameObject flash = _newBestText.gameObject;

        while (_isShowing)
        {
            flash.SetActive(!flash.activeSelf);
            yield return wait;
        }
    }
}