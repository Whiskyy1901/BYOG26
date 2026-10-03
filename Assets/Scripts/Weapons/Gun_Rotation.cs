using UnityEngine;
using UnityEngine.InputSystem;

public class Gun_Rotation : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _orbitPlayer = true;

    [Header("Orbit Settings")]
    [Tooltip("What the gun orbits around. Defaults to the parent (the player).")]
    [SerializeField] private Transform _orbitCenter;
    [SerializeField] private float _orbitRadius = 0.8f;
    [Tooltip("Flip the sprite vertically when aiming left so the gun isn't upside down.")]
    [SerializeField] private bool _flipWhenAimingLeft = true;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_orbitCenter == null && transform.parent != null)
            _orbitCenter = transform.parent;
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (Mouse.current == null) return;

        Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Orbit mode aims from the player's centre, pivot mode aims from the gun itself
        bool orbiting = _orbitPlayer && _orbitCenter != null;
        Vector2 origin = orbiting ? (Vector2)_orbitCenter.position : (Vector2)transform.position;
        Vector2 direction = mouseWorldPos - origin;

        if (direction.sqrMagnitude < 0.0001f) return; // mouse is exactly on the origin

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (orbiting)
            transform.position = origin + direction.normalized * _orbitRadius;

        if (_flipWhenAimingLeft)
        {
            Vector3 scale = transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (direction.x < 0f ? -1f : 1f);
            transform.localScale = scale;
        }
    }
}