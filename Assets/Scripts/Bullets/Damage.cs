using System;
using UnityEngine;

public class Damage : MonoBehaviour
{
    private float _damage;
    
    public void Initialization(float damage)
    {
        _damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Health>(out Health health))
            {
                health.Damage(_damage);
            }
        }

        Destroy(this.gameObject);
    }
}
