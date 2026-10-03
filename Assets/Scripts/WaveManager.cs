using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyEntry
{
    public GameObject obj;
    [Min(1)] public int value = 1;
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    [SerializeField] private List<EnemyEntry> _enemies = new List<EnemyEntry>();

    [Header("Wave budget")]
    [SerializeField] private int _baseWaveValue = 25;
    [SerializeField] private int _waveValueIncrease = 100;
    [SerializeField] private float _waveCooldown = 5f;

    [Header("Spawn ring around the player")]
    [Tooltip("Keep this larger than the distance from the player to the screen edge.")]
    [SerializeField] private float _minSpawnDistance = 8f;
    [SerializeField] private float _maxSpawnDistance = 12f;

    private readonly HashSet<GameObject> _waveSpawns = new HashSet<GameObject>();
    private readonly List<EnemyEntry> _affordable = new List<EnemyEntry>(); // reused to avoid allocations

    private int _waveNumber;
    private float _currentCooldown;

    public event Action<GameObject> OnEnemyDefeated;

    private void Update()
    {
        if (_waveSpawns.Count > 0) return;

        _currentCooldown += Time.deltaTime;
        if (_currentCooldown < _waveCooldown) return;

        _currentCooldown = 0f;
        _waveNumber++; // wave 1 is the first wave
        SpawnWave(_baseWaveValue + (_waveNumber - 1) * _waveValueIncrease);
    }

    private void SpawnWave(int waveValue)
    {
        int budget = waveValue;

        // Keep buying random affordable enemies until nothing fits in the leftover budget.
        while (true)
        {
            _affordable.Clear();
            foreach (var entry in _enemies)
            {
                if (entry.obj != null && entry.value > 0 && entry.value <= budget)
                    _affordable.Add(entry);
            }

            if (_affordable.Count == 0) break;

            EnemyEntry pick = _affordable[Random.Range(0, _affordable.Count)];
            budget -= pick.value;
            Spawn(pick.obj, RandomPosition());
        }
    }

    public GameObject Spawn(GameObject enemy, Vector2 position)
    {
        GameObject obj = Instantiate(enemy, position, Quaternion.identity);

        if (obj.TryGetComponent(out BasicEnemy basicEnemy))
            basicEnemy.Initialization(_player);

        if (!obj.TryGetComponent(out DestroyTracker tracker))
            tracker = obj.AddComponent<DestroyTracker>();
        tracker.manager = this;

        _waveSpawns.Add(obj);
        return obj;
    }

    public void NotifyDestroyed(GameObject obj)
    {
        _waveSpawns.Remove(obj);
        OnEnemyDefeated?.Invoke(obj);
    }

    // Random point on a ring around the player, so enemies never spawn on top of them.
    public Vector2 RandomPosition()
    {
        Vector2 center = _player != null ? (Vector2)_player.transform.position : (Vector2)transform.position;
        Vector2 direction = Random.insideUnitCircle.normalized;
        if (direction == Vector2.zero) direction = Vector2.right; // extremely rare edge case

        float distance = Random.Range(_minSpawnDistance, _maxSpawnDistance);
        return center + direction * distance;
    }
}