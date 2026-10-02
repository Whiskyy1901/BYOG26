using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 5;
    private float _currenthealth;

    private void Awake()
    {
        _currenthealth = _maxHealth;
    }

    public void Damage(float damage)
    {
        if (_currenthealth - damage <= 0)
        {
            _currenthealth = 0;
            DeathSequence();
        }
        else
        {
            _currenthealth -= damage;
        }
    }

    public void Heal(float heal)
    {
        if (_currenthealth + heal >= _maxHealth)
        {
            _currenthealth = _maxHealth;
        }
        else
        {
            _currenthealth += heal;
        }
    }

    private void DeathSequence()
    {
        // Play animations, audio
        Destroy(this.gameObject);
    }
}
