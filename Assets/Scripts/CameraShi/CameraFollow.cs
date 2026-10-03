using UnityEngine;
using UnityEngine.InputSystem;
 
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
     public static CameraFollow Instance { get; private set; }
 
    [Header("Follow")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Look Ahead")]
    [SerializeField] private bool _lookTowardMouse = true;
    [SerializeField, Range(0f, 0.5f)] private float _lookAmount = 0.15f;
    [Header("Bounds (optional)")]
    [SerializeField] private bool _useBounds = false;
    [SerializeField] private Vector2 _minBounds = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 _maxBounds = new Vector2(10f, 10f);
 
    [Header("Shake")]
    [SerializeField] private float _shakePerDamage = 0.02f;
    [SerializeField, Range(0f, 1f)] private float _pelletAmplify = 0.25f;
    [SerializeField] private float _maxShake = 0.6f;
    [SerializeField] private float _shakeDecay = 2.5f;
    [SerializeField] private float _shakeFrequency = 35f;

    private Camera _cam;
    private Vector3 _basePosition; // where the follow logic wants the camera, with no shake applied
    private Vector3 _velocity;
    private float _z;
    private float _currentShake;
    private float _noiseSeedX;
    private float _noiseSeedY;

    private void Awake()
    {
        Instance = this;
        _cam = GetComponent<Camera>();
        _z = transform.position.z; // 2D camera depth never changes
 
        _noiseSeedX = Random.Range(0f, 100f);
        _noiseSeedY = Random.Range(100f, 200f);
 
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _target = player.transform;
            else Debug.LogWarning("CameraFollow: no target assigned and no object tagged 'Player' found.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
 
    private void Start()
    {
        _basePosition = transform.position;
 
        // Start centred on the player instead of sweeping in from wherever the camera was placed
        if (_target != null)
        {
            _basePosition = ToWorld(GetDesiredPosition());
            transform.position = _basePosition;
        }
    }

 
    // LateUpdate so the player has already moved this frame (prevents jitter)
    private void LateUpdate()
    {
        if (_target != null)
        {
            // Follow is smoothed on the base position, never on the shaken transform,
            // otherwise the shake would feed back into the follow and drift
            _basePosition = Vector3.SmoothDamp(
                _basePosition, ToWorld(GetDesiredPosition()), ref _velocity, _smoothTime);
        }
 
        transform.position = _basePosition + (Vector3)GetShakeOffset();
    }

    public void Shake(float amount)
    {
        _currentShake = Mathf.Min(_maxShake, _currentShake + amount);
    }

    public void ShakeFromShot(float damage, int pellets)
    {
        float pelletMultiplier = 1f + Mathf.Max(0, pellets - 1) * _pelletAmplify;
        Shake(damage * _shakePerDamage * pelletMultiplier);
    }
 
    private Vector2 GetShakeOffset()
    {
        if (_currentShake <= 0f) return Vector2.zero;
 
        _currentShake = Mathf.MoveTowards(_currentShake, 0f, _shakeDecay * Time.deltaTime);
 
        
        float t = Time.time * _shakeFrequency;
        float x = Mathf.PerlinNoise(t, _noiseSeedX) * 2f - 1f;
        float y = Mathf.PerlinNoise(t, _noiseSeedY) * 2f - 1f;
 
        return new Vector2(x, y) * _currentShake;
    }

    private Vector2 GetDesiredPosition()
    {
        Vector2 desired = _target.position;
 
        if (_lookTowardMouse && Mouse.current != null)
        {
            Vector2 viewport = _cam.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            viewport = new Vector2(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y));
 
            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;
 
            desired += new Vector2(
                (viewport.x - 0.5f) * 2f * halfWidth,
                (viewport.y - 0.5f) * 2f * halfHeight) * _lookAmount;
        }
 
        if (_useBounds)
            desired = ClampToBounds(desired);
 
        return desired;
    }

    private Vector2 ClampToBounds(Vector2 position)
    {
        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;
 
        Vector2 min = _minBounds + new Vector2(halfWidth, halfHeight);
        Vector2 max = _maxBounds - new Vector2(halfWidth, halfHeight);
 
        // If the area is smaller than the view on an axis, just centre on it
        position.x = min.x > max.x ? (_minBounds.x + _maxBounds.x) * 0.5f : Mathf.Clamp(position.x, min.x, max.x);
        position.y = min.y > max.y ? (_minBounds.y + _maxBounds.y) * 0.5f : Mathf.Clamp(position.y, min.y, max.y);
        return position;
    }

    private Vector3 ToWorld(Vector2 position) => new Vector3(position.x, position.y, _z);
}