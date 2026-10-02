using System;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private GameObject _player;
    private Rigidbody2D _rb;
    
    [SerializeField] private float _speed;
    [SerializeField] private float _damage;

    [SerializeField] private float _timeBetweenAttacks;
    private float _time;

    public void Initialization(GameObject player)
    {
        _player = player;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //move towards player
        _rb.linearVelocity = (_player.transform.position - transform.position).normalized * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.transform.root.TryGetComponent<Health>(out Health health) &&
                _time > _timeBetweenAttacks)
            {
                health.Damage(_damage);
                _time = 0;
            }
        }
    }
}
