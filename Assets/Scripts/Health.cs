using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 5;
    private float _currentHealth;
    private bool _isDead;
    private bool _isPlayer;

    [Header("For Player")]
    [SerializeField] Health_Bar _healthBar;
    [SerializeField] private float _shakeAmount = 0.25f;     // added to CameraFollow's shake (it clamps to its own max)
    [SerializeField] private float _vignetteStrength = 0.4f; // how much the vignette pulses on a hit
    [SerializeField] private Color _playerFlashColor = new Color(1f, 0.92f, 0.2f, 1f); // player's own hit flash colour

    [Header("Passive Regen (Player only)")]
    [SerializeField] private bool _passiveRegen = true;
    [Tooltip("Seconds without taking damage before regen starts.")]
    [SerializeField] private float _regenDelay = 3f;
    [Tooltip("HP restored per second while regenerating.")]
    [SerializeField] private float _regenPerSecond = 0.5f;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer[] _renderers; // leave empty to auto-find on this object and children
    [SerializeField] private Color _flashColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float _flashDuration = 0.1f;

    // SFX
    [Header("Sounds")]
    [Tooltip("Plays when damaged but not killed.")]
    [SerializeField] private SoundEffect _hitSound = new SoundEffect();
    [SerializeField] private SoundEffect _deathSound = new SoundEffect();
    [Tooltip("Plays for direct Heal() calls (e.g. health pickups). Passive regen uses the sounds below instead.")]
    [SerializeField] private SoundEffect _healSound = new SoundEffect();

    [Header("Regen Sounds (Player only)")]
    [Tooltip("Plays once when regen kicks in after the delay.")]
    [SerializeField] private SoundEffect _regenStartSound = new SoundEffect();
    [Tooltip("Plays every time regen restores this much HP. 0 = no tick sound.")]
    [SerializeField] private float _regenTickAmount = 1f;
    [SerializeField] private SoundEffect _regenTickSound = new SoundEffect();
    [Tooltip("Plays when regen tops the player back up to full.")]
    [SerializeField] private SoundEffect _regenFullSound = new SoundEffect();

    private Color[] _originalColors;
    private Coroutine _flashRoutine;

    private float _timeSinceDamage;
    private bool _isRegenerating;
    private float _regenTickProgress;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;
    public bool IsRegenerating => _isRegenerating; // handy for a UI effect on the health bar

    public event Action<float> OnDamaged; // amount of damage taken (hit flashes, UI)
    public event Action<Health> OnDeath;  // passes itself, so a drop system can read the position

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _isPlayer = gameObject.CompareTag("Player");

        if (_isPlayer)
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }

        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<SpriteRenderer>();

        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].color;
    }

    private void Update()
    {
        if (_isPlayer)
        {
            HandleRegen();
            _healthBar.SetHealth(_currentHealth);
        }
    }

    private void HandleRegen()
    {
        if (!_passiveRegen || _isDead) return;

        _timeSinceDamage += Time.deltaTime;

        if (_currentHealth >= _maxHealth)
        {
            _isRegenerating = false;
            return;
        }

        if (_timeSinceDamage < _regenDelay) return;

        if (!_isRegenerating)
        {
            _isRegenerating = true;
            _regenTickProgress = 0f;
            SoundManager.Play(_regenStartSound);
        }

        float amount = Mathf.Min(_regenPerSecond * Time.deltaTime, _maxHealth - _currentHealth);
        _currentHealth += amount;

        if (_currentHealth >= _maxHealth)
        {
            _currentHealth = _maxHealth;
            _isRegenerating = false;
            SoundManager.Play(_regenFullSound); // replaces the tick on the final frame
            return;
        }

        if (_regenTickAmount > 0f)
        {
            _regenTickProgress += amount;
            if (_regenTickProgress >= _regenTickAmount)
            {
                _regenTickProgress -= _regenTickAmount;
                SoundManager.Play(_regenTickSound);
            }
        }
    }

    public void Damage(float damage)
    {
        if (_isDead) return; // several bullets landing on the same frame can't kill twice

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnDamaged?.Invoke(damage);

        // Taking damage resets the regen delay and stops any regen in progress
        _timeSinceDamage = 0f;
        _isRegenerating = false;

        PlayHitFeedback();

        if (_currentHealth > 0f)
            SoundManager.Play(_hitSound); // SFX (the killing hit plays the death sound instead)

        if (_currentHealth <= 0f)
            DeathSequence();
    }

    public void Heal(float heal)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + heal);
        SoundManager.Play(_healSound); // SFX
    }

    private void PlayHitFeedback()
    {
        // Everything flashes
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(Flash());

        // Only the player shakes the camera and pulses the vignette
        if (!_isPlayer) return;

        if (CameraFollow.Instance != null)
            CameraFollow.Instance.Shake(_shakeAmount);

        if (DamageVignette.Instance != null)
            DamageVignette.Instance.Pulse(_vignetteStrength);
    }

    private IEnumerator Flash()
    {
        Color flashColor = _isPlayer ? _playerFlashColor : _flashColor;

        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = flashColor;

        yield return new WaitForSeconds(_flashDuration);

        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = _originalColors[i];

        _flashRoutine = null;
    }

    private void DeathSequence()
    {
        _isDead = true;
        OnDeath?.Invoke(this); // fires before Destroy, so subscribers can still use transform.position
        // Play animations, audio
        SoundManager.Play(_deathSound); // SFX (SoundManager outlives this object, so the sound isn't cut off)
        if (this.gameObject.CompareTag("Player"))
        {
            FindAnyObjectByType<GameOverScreen>().Show();
        }
        Destroy(gameObject);
    }
}