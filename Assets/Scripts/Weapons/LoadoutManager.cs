using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadoutManager : MonoBehaviour
{
    private InputHandler _input;
    private int _currentIndex = 0;

    [SerializeField] private List<GameObject> _guns = new List<GameObject>();
    [SerializeField] private List<GameObject> _activeGuns = new List<GameObject>();

    [Tooltip("Pickup prefab for each gun. Must be in the same order as the _guns list.")]
    [SerializeField] private List<GameObject> _pickupPrefabs = new List<GameObject>();

    // Timed guns only. The starter gun (index 0 of _activeGuns) is never in here, so it never expires.
    private readonly Dictionary<GameObject, float> _expiryTimes = new Dictionary<GameObject, float>();
    private readonly List<GameObject> _expiredBuffer = new List<GameObject>();
    private readonly List<int> _candidateBuffer = new List<int>();

    public int GunCount => _guns.Count;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
        _activeGuns.Clear();
        _expiryTimes.Clear();
        foreach (var gun in _guns)
        {
            gun.SetActive(false);
        }
    }

    private void Start()
    {
        // Assign random starter gun (permanent)
        int gunPos = Random.Range(0, _guns.Count);
        _activeGuns.Add(_guns[gunPos]);
        _activeGuns[0].SetActive(true);
    }

    private void Update()
    {
        CheckExpiry();

        if (_input.SwitchAction.WasPressedThisFrame())
        {
            _currentIndex = (_currentIndex + 1) % _activeGuns.Count;
            SwitchGuns(_currentIndex);
        }
    }

    private void SwitchGuns(int index)
    {
        for (int i = 0; i < _activeGuns.Count; i++)
        {
            _activeGuns[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// Returns a random index into _guns for a gun the player does not currently hold.
    /// Returns -1 if the player already holds every gun.
    /// </summary>
    public int GetRandomUnownedGunIndex()
    {
        _candidateBuffer.Clear();
        for (int i = 0; i < _guns.Count; i++)
        {
            if (!_activeGuns.Contains(_guns[i]))
                _candidateBuffer.Add(i);
        }

        if (_candidateBuffer.Count == 0) return -1;
        return _candidateBuffer[Random.Range(0, _candidateBuffer.Count)];
    }

    /// <summary>
    /// Returns the pickup prefab that matches the gun at the given index, or null if none is set.
    /// </summary>
    public GameObject GetPickupPrefab(int gunIndex)
    {
        if (gunIndex < 0 || gunIndex >= _pickupPrefabs.Count) return null;
        return _pickupPrefabs[gunIndex];
    }

    /// <summary>
    /// Adds a gun from the _guns list for a limited time and equips it.
    /// Picking up a gun you already hold refreshes its timer.
    /// </summary>
    public void AddGun(int gunIndex, float duration)
    {
        if (gunIndex < 0 || gunIndex >= _guns.Count) return;

        GameObject gun = _guns[gunIndex];
        int index = _activeGuns.IndexOf(gun);

        if (index < 0)
        {
            _activeGuns.Add(gun);
            index = _activeGuns.Count - 1;
            _expiryTimes[gun] = Time.time + duration;
        }
        else if (_expiryTimes.ContainsKey(gun))
        {
            // Already held as a timed gun: refresh the timer
            _expiryTimes[gun] = Time.time + duration;
        }
        // else: it's the permanent starter gun, so no timer is needed

        _currentIndex = index;
        SwitchGuns(_currentIndex);
    }

    private void CheckExpiry()
    {
        if (_expiryTimes.Count == 0) return;

        _expiredBuffer.Clear();
        foreach (var pair in _expiryTimes)
        {
            if (Time.time >= pair.Value)
                _expiredBuffer.Add(pair.Key);
        }

        foreach (var gun in _expiredBuffer)
        {
            RemoveGun(gun);
        }
    }

    private void RemoveGun(GameObject gun)
    {
        _expiryTimes.Remove(gun);

        int index = _activeGuns.IndexOf(gun);
        if (index < 0) return;

        _activeGuns.RemoveAt(index);
        gun.SetActive(false);

        if (index == _currentIndex)
            _currentIndex = 0;          // fall back to the starter gun
        else if (index < _currentIndex)
            _currentIndex--;            // keep pointing at the same gun

        SwitchGuns(_currentIndex);
    }
}