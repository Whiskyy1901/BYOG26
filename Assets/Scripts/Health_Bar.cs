using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health_Bar : MonoBehaviour
{
    private Slider _slider;
    public Gradient gradient;
    public Image fill;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void SetMaxHealth(float maxHealth)
    {
        _slider.maxValue = maxHealth;
        _slider.value = maxHealth;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        _slider.value = health;
        fill.color = gradient.Evaluate(_slider.normalizedValue);
    }
}
