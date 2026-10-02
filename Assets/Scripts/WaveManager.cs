using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyEntry
{
    public GameObject obj;
    public int value;
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    
    [SerializeField] private List<EnemyEntry> enemies = new List<EnemyEntry>();
    private List<GameObject> waveSpawns = new List<GameObject>();

    [SerializeField] private float _spawnRadius = 5;
    [SerializeField] private float _waveCooldown = 5;
    
    private int _waveNumber = 0;
    private int _waveValue;
    private float _currentCooldown;
    public event Action<GameObject> OnEnemyDefeated;
    
    private void Awake()
    {
        _waveValue = 25 + _waveNumber * 100;
    }

    private void Update()
    {
        if (waveSpawns.Count == 0)
        {
            _currentCooldown += Time.deltaTime;
            if (_currentCooldown >= _waveCooldown)
            {
                _waveValue = 25 + _waveNumber * 100;
                _waveNumber++;
                SpawnWave(_waveValue);
                _currentCooldown = 0;
            }
        }
    }

    private void SpawnWave(int waveValue)
    {
        foreach (var enemy in enemies)
        {
            int spawnCount = Random.Range(0, waveValue / enemy.value);
            waveValue -= spawnCount * enemy.value;
            for (int i = 0; i < spawnCount; i++)
            {
                Spawn(enemy.obj, RandomPosition()); // Add Random place spawning.
            }
        }
    }
    
    public GameObject Spawn(GameObject enemy,Vector2 position)
    {
        GameObject obj = Instantiate(enemy, position, Quaternion.identity);
        
        if(obj.TryGetComponent<BasicEnemy>(out BasicEnemy basicEnemy))
            basicEnemy.Initialization(_player);
        
        var tracker = obj.AddComponent<DestroyTracker>();
        tracker.manager = this;

        waveSpawns.Add(obj);
        return obj;
    }
    
    public void NotifyDestroyed(GameObject obj)
    {
        waveSpawns.Remove(obj);
        OnEnemyDefeated?.Invoke(obj);
    }

    public Vector2 RandomPosition()
    {
        return Random.insideUnitCircle * _spawnRadius;
    }
}
