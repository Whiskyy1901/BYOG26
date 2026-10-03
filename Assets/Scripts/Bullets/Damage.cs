using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] private LayerMask _toDamage;
    [SerializeField] private float _lifetime = 5f;

    private float _damage;
    private bool _hasHit;

    public void Initialization(float damage)
    {
        _damage = damage;
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime); // stray bullets don't live forever offscreen
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return; // ignore a second overlap on the same frame
        _hasHit = true;

        // A LayerMask is a bitmask, so test the layer's bit instead of comparing numbers
        bool isTarget = (_toDamage.value & (1 << other.gameObject.layer)) != 0;

        if (isTarget)
        {
            // InParent, because the collider is often on a child while Health sits on the root
            Health health = other.GetComponentInParent<Health>();
            if (health != null)
                health.Damage(_damage);
        }

        // Anything the Layer Collision Matrix lets us touch (target or wall) stops the bullet
        Destroy(gameObject);
    }
}