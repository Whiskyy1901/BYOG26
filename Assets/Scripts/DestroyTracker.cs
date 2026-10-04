using UnityEngine;

public class DestroyTracker : MonoBehaviour
{
    [HideInInspector] public WaveManager manager;

    [Header("Drops")]
    [SerializeField, Range(0f, 1f)] private float _dropChance = 0.1f;

    private bool _quitting;

    private void OnApplicationQuit()
    {
        _quitting = true;
    }

    private void OnDestroy()
    {
        if (manager != null)
            manager.NotifyDestroyed(gameObject);

        TryDropWeapon();
    }

    private void TryDropWeapon()
    {
        // Don't spawn while the game is closing or the scene is unloading
        if (_quitting || !gameObject.scene.isLoaded) return;
        if (Random.value > _dropChance) return;

        LoadoutManager loadout = FindAnyObjectByType<LoadoutManager>();
        if (loadout == null) return;

        // Pick a gun the player doesn't hold, then drop that gun's own pickup prefab
        int gunIndex = loadout.GetRandomUnownedGunIndex();
        if (gunIndex < 0) return;

        GameObject prefab = loadout.GetPickupPrefab(gunIndex);
        if (prefab == null) return;

        GameObject drop = Instantiate(prefab, transform.position, Quaternion.identity);

        if (drop.TryGetComponent(out PickupItem pickup))
            pickup.SetGunIndex(gunIndex);
    }
}