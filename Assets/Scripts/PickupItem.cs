using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private bool _isWeapon = true;
    [SerializeField] private bool _isAbility = false;

    [Header("Weapon settings")]
    [Tooltip("Index into LoadoutManager's gun list. -1 picks a random gun when the pickup spawns.")]
    [SerializeField] private int _gunIndex = -1;
    [SerializeField] private float _weaponDuration = 15f;

    [Header("Pickup lifetime")]
    [Tooltip("Seconds before an uncollected pickup disappears. 0 = never.")]
    [SerializeField] private float _lifetime = 10f;

    // SFX
    [Header("Sounds")]
    [SerializeField] private SoundEffect _pickupSound = new SoundEffect();

    private LoadoutManager _loadoutManager;

    public void SetGunIndex(int gunIndex)
    {
        _gunIndex = gunIndex;
    }

    private void Start()
    {
        _loadoutManager = FindAnyObjectByType<LoadoutManager>();

        if (_isWeapon && _gunIndex < 0 && _loadoutManager != null)
        {
            _gunIndex = _loadoutManager.GetRandomUnownedGunIndex();

            // Player already holds every gun, so there is nothing new to drop
            if (_gunIndex < 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (_lifetime > 0f)
            Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (_loadoutManager == null) return;

        if (_isWeapon)
        {
            _loadoutManager.AddGun(_gunIndex, _weaponDuration);
            SoundManager.Play(_pickupSound); // SFX
            Destroy(gameObject);
        }
        else if (_isAbility)
        {
            //Add later
        }
    }
}