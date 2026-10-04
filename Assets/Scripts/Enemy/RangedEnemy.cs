using System.Collections;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged")]
    [SerializeField] private float _shootRange = 7f;
    [SerializeField] private float _fireInterval = 1.5f;
    [SerializeField] private Transform _firePos; // optional, falls back to the enemy's position

    [Header("Projectile")]
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletSpeed = 8f;
    [SerializeField] private float _spread = 3f; 

    [Header("Burst (1 = single shot)")]
    [SerializeField] private int _burstCount = 1;
    [SerializeField] private float _burstInterval = 0.1f;

    // SFX
    [Header("Ranged Sounds")]
    [Tooltip("Plays for every bullet, so a burst plays it once per shot.")]
    [SerializeField] private SoundEffect _shootSound = new SoundEffect();

    private float _fireTimer;
    private bool _isBursting;

    protected override void Awake()
    {
        base.Awake();
        _damageOnContact = false; // this enemy only hurts with bullets
    }

    protected override void Behave()
    {
        // Walk until inside shooting range
        if (DistanceToPlayer > _shootRange)
        {
            _rb.linearVelocity = DirectionToPlayer * _speed;
            return;
        }

        Stop();

        _fireTimer += Time.fixedDeltaTime;
        if (_fireTimer >= _fireInterval && !_isBursting)
        {
            _fireTimer = 0f;
            StartCoroutine(FireBurst());
        }
    }

    private IEnumerator FireBurst()
    {
        _isBursting = true;

        for (int i = 0; i < _burstCount; i++)
        {
            if (_player == null) break;
            Shoot();
            yield return new WaitForSeconds(_burstInterval);
        }

        _isBursting = false;
    }

    private void Shoot()
    {
        Vector2 origin = _firePos != null ? (Vector2)_firePos.position : (Vector2)transform.position;
        Vector2 dir = (Vector2)_player.transform.position - origin;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
                      + Random.Range(-_spread * 0.5f, _spread * 0.5f);

        GameObject bullet = Instantiate(_bullet, origin, Quaternion.Euler(0f, 0f, angle));
        SoundManager.Play(_shootSound); // SFX

        if (bullet.TryGetComponent<Damage>(out var damage))
            damage.Initialization(_damage);

        if (bullet.TryGetComponent<Move_Forward>(out var move))
            move.Initialization(_bulletSpeed);
    }
}