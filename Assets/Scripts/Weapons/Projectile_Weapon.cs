using UnityEngine;
using UnityEngine.InputSystem;

public enum FireType {Manual, Automatic}

public class Projectile_Weapon : MonoBehaviour
{
    [Header("Weapon Details")]
    [SerializeField] private float _fireRate = 1f;
    private float _fireInterval = 1;
    private float _timeBetweenFire;
    [SerializeField] private Transform _firePos;
    
    [Header("Projectile")]
    [SerializeField] private int _pelletCount = 1;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _damage = 1f;

    [Header("Bullet Spread")]
    [SerializeField] private float minSpread = 2f;
    [SerializeField] private float maxSpread = 12f;
    [SerializeField] private float bloomPerShot = 1.5f;
    [SerializeField] private float bloomRecovery = 8f;   // degrees per second
    [SerializeField] private bool clusterTowardCenter = true;
    private float currentSpread;

    [Header("Firetype")] [SerializeField] private FireType _fireType;

    // SFX
    [Header("Sounds")]
    [Tooltip("Plays once per trigger pull, not once per pellet.")]
    [SerializeField] private SoundEffect _shootSound = new SoundEffect();

    [Header("Muzzle Flash")]
    [Tooltip("Prefab spawned at the fire position each shot (particles, a flash sprite, etc). Leave empty for none.")]
    [SerializeField] private GameObject _muzzleFlash;
    [Tooltip("Parent the flash to the fire position so it moves with the gun while it plays.")]
    [SerializeField] private bool _attachFlashToGun = true;
    [Tooltip("Seconds before the spawned flash is destroyed, as a safety net if the prefab doesn't clean itself up.")]
    [SerializeField] private float _muzzleFlashLifetime = 1f;
    
    private InputAction _fireAction;
    
    private void Awake()
    {
        _fireAction = InputSystem.actions.FindAction(("Attack"));
        _fireInterval = 1 / _fireRate;
        currentSpread = minSpread;
    }

    private void Update()
    {
        _timeBetweenFire += Time.deltaTime;
        currentSpread = Mathf.MoveTowards(currentSpread, minSpread, bloomRecovery * Time.deltaTime);

        if (_fireAction.IsPressed() && _timeBetweenFire >= _fireInterval && _fireType == FireType.Automatic)
        {
            Shoot();
            _timeBetweenFire = 0;
        }
        else if(_fireAction.WasPressedThisFrame() && _timeBetweenFire >= _fireInterval && _fireType == FireType.Manual)
        {
            Shoot();
            _timeBetweenFire = 0;
        }
    }

    private void Shoot()
    {
        SoundManager.Play(_shootSound); // SFX
        SpawnMuzzleFlash(_firePos.rotation); // VFX: once per trigger pull, not per pellet

        // One shake per trigger pull (the camera scales it by pellet count itself)
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.ShakeFromShot(_damage, _pelletCount);

        // Every pellet in this shot uses the same spread value
        float spread = currentSpread;

        for (int i = 0; i < _pelletCount; i++)
        {
            float offset = GetSpreadOffset(spread);
            Quaternion rotation = _firePos.rotation * Quaternion.Euler(0f, 0f, offset);
            GameObject bullet = Instantiate(_bullet, _firePos.position, rotation);

            if (bullet.TryGetComponent<Damage>(out var damage))
            {
                damage.Initialization(_damage);
            }
            
            if (bullet.TryGetComponent<Move_Forward>(out var move))
            {
                move.Initialization(_bulletSpeed * Random.Range(0.85f, 1.15f));
            }
        }

        // Bloom grows once per shot, not once per pellet
        currentSpread = Mathf.Min(currentSpread + bloomPerShot, maxSpread);
    }
    
    private void SpawnMuzzleFlash(Quaternion rotation)
    {
        if (_muzzleFlash == null || _firePos == null) return;

        GameObject flash = _attachFlashToGun
            ? Instantiate(_muzzleFlash, _firePos.position, rotation, _firePos)
            : Instantiate(_muzzleFlash, _firePos.position, rotation);

        if (_muzzleFlashLifetime > 0f)
            Destroy(flash, _muzzleFlashLifetime);
    }

    private float GetSpreadOffset(float spread)
    {
        float half = spread * 0.5f;
 
        if (clusterTowardCenter)
        {
            // Average of two randoms gives a triangular distribution
            float a = Random.Range(-half, half);
            float b = Random.Range(-half, half);
            return (a + b) * 0.5f;
        }
 
        return Random.Range(-half, half);
    }
}