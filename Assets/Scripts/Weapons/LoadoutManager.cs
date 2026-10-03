using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadoutManager : MonoBehaviour
{
    private InputHandler _input;
    private int _currentIndex = 0;
    
    [SerializeField] private List<GameObject> _guns = new List<GameObject>();
    [SerializeField] private List<GameObject> _activeGuns = new List<GameObject>();

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
        _activeGuns.Clear();
        foreach (var gun in _guns)
        {
           gun.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Assign random gun
        int gunPos = Random.Range(0, _guns.Count);
        _activeGuns.Add(_guns[gunPos]);
        _activeGuns[0].gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_input.SwitchAction.WasPressedThisFrame())
        {
            _currentIndex = (_currentIndex + 1) % _activeGuns.Count;
            SwitchGuns(_currentIndex);
        }
    }

    void SwitchGuns(int index)
    {
        for (int i = 0; i < _activeGuns.Count; i++)
        {
            _activeGuns[i].SetActive(i == index);
        }
    }
}
