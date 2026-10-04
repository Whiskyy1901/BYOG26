using System;
using UnityEngine;
using TMPro;

public class WaveCount : MonoBehaviour
{
    private WaveManager _waveManager;
    [SerializeField] private TextMeshProUGUI _waveText;

    private void Start()
    {
        _waveManager = FindAnyObjectByType<WaveManager>();
    }

    private void Update()
    {
        _waveText.text = "Wave: " + _waveManager.WaveNumber;
    }
}
