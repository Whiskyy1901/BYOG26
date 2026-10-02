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
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _damage = 1f;

    [Header("Bullet Spread")]
    [SerializeField] private float _spreadAngle = 0;

    [Header("Firetype")] [SerializeField] private FireType _fireType;
    
    private InputAction _fireAction;
    
    private void Awake()
    {
        _fireAction = InputSystem.actions.FindAction(("Attack"));
        _fireInterval = 1 / _fireRate;
    }

    private void Update()
    {
        _timeBetweenFire += Time.deltaTime;

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
        GameObject bullet = Instantiate(_bullet, _firePos.position, _firePos.rotation);

        if (bullet.TryGetComponent<Damage>(out var damage))
        {
            damage.Initialization(_damage);
        }
        
        if (bullet.TryGetComponent<Move_Forward>(out var move))
        {
            move.Initialization(_bulletSpeed);
        }
    }
}
