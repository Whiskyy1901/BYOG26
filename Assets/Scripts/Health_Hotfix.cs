using UnityEngine;

public class Health_Hotfix : MonoBehaviour
{
    [SerializeField] private Health _health;

    private void Awake()
    {
        if (_health == null) _health = GetComponentInParent<Health>();
    }

    private void LateUpdate()
    {
        if (_health != null && !_health.enabled)
        {
            Debug.LogWarning("Health was disabled, re-enabling.", this);
            _health.enabled = true;
        }
    }
}
