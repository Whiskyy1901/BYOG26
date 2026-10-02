using UnityEngine;
using UnityEngine.InputSystem;

public class Hitscan_Weapon : MonoBehaviour
{
    [Header("Weapon Details")]
    [SerializeField] private float _fireRate = 1f;
    private float _fireInterval = 1;
    private float _timeBetweenFire;
    [SerializeField] private Transform _firePos;
    
    [Header("Raycast")]
    [SerializeField] private float _range;
    [SerializeField] private float _damage;

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
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(_firePos.position, transform.right, _range);

        foreach (RaycastHit2D hit in hits2D)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.gameObject.TryGetComponent<Health>(out var health);
                health.Damage(_damage);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_firePos.position, _firePos.position + transform.right * _range);
    }
}
