using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 5;
    private float _currentHealth;
    private bool _isDead;

    [Header("For Player")]
    [SerializeField] Health_Bar _healthBar;
    
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _isDead;

    public event Action<float> OnDamaged; // amount of damage taken (hit flashes, UI)
    public event Action<Health> OnDeath;  // passes itself, so a drop system can read the position

    private void Awake()
    {
        _currentHealth = _maxHealth;
        if (this.gameObject.CompareTag("Player"))
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }
    }

    private void Update()
    {
        if (this.gameObject.CompareTag("Player"))
        {
            _healthBar.SetHealth(_currentHealth);
        }
    }

    public void Damage(float damage)
    {
        if (_isDead) return; // several bullets landing on the same frame can't kill twice

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnDamaged?.Invoke(damage);

        if (_currentHealth <= 0f)
            DeathSequence();
    }

    public void Heal(float heal)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + heal);
    }

    private void DeathSequence()
    {
        _isDead = true;
        OnDeath?.Invoke(this); // fires before Destroy, so subscribers can still use transform.position
        // Play animations, audio
        Destroy(gameObject);
    }
}