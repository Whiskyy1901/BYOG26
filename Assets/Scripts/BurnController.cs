using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BurnController : MonoBehaviour
{
    public enum GameState { Ready, Playing, Paused, GameOver }

    [Header("References")]
    public BurningPaper paper;
    public Transform[] players;

    [Header("Flow")]
    public bool autoStart = true;
    public float startDelay = 2f;
    public bool endWhenAllPlayersFall = false;

    [Header("UI (optional)")]
    public Slider paperRemainingBar;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    [Header("Events")]
    public UnityEvent onGameStarted;
    public UnityEvent onGameOver;
    public UnityEvent<Transform> onPlayerFell;

    public GameState State { get; private set; } = GameState.Ready;
    public float SurvivalTime { get; private set; }

    readonly HashSet<Transform> fallen = new HashSet<Transform>();

    void Start()
    {
        Time.timeScale = 1f;
        SetActive(pauseScreen, false);
        SetActive(gameOverScreen, false);
        paper.onPaperBurned.AddListener(EndGame);
        UpdateUI();

        if (autoStart) StartCoroutine(StartAfterDelay());
    }

    void Update()
    {
        if (PausePressed()) TogglePause();
        if (RestartPressed() && State == GameState.GameOver) Restart();

        if (State != GameState.Playing) return;

        SurvivalTime += Time.deltaTime;
        CheckPlayers();
        UpdateUI();
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartGame();
    }

    public void StartGame()
    {
        if (State != GameState.Ready) return;

        State = GameState.Playing;
        paper.StartBurning();
        onGameStarted.Invoke();
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) SetPaused(true);
        else if (State == GameState.Paused) SetPaused(false);
    }

    public void SetPaused(bool paused)
    {
        if (State != GameState.Playing && State != GameState.Paused) return;

        State = paused ? GameState.Paused : GameState.Playing;
        Time.timeScale = paused ? 0f : 1f;
        SetActive(pauseScreen, paused);
    }

    public void SetBurnSpeed(float multiplier)
    {
        paper.SpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void EndGame()
    {
        if (State == GameState.GameOver) return;

        State = GameState.GameOver;
        paper.Pause();
        UpdateUI();
        SetActive(gameOverScreen, true);
        onGameOver.Invoke();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void CheckPlayers()
    {
        foreach (Transform player in players)
        {
            if (player == null || fallen.Contains(player)) continue;

            if (paper.IsBurnedAt(player.position))
            {
                fallen.Add(player);
                onPlayerFell.Invoke(player);
            }
        }

        if (endWhenAllPlayersFall && players.Length > 0 && fallen.Count >= players.Length)
            EndGame();
    }

    void UpdateUI()
    {
        if (paperRemainingBar != null)
            paperRemainingBar.value = paper.RemainingFraction;
    }

    static void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    static bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    static bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        if (paper != null) paper.onPaperBurned.RemoveListener(EndGame);
    }
}
