using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(InputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 80f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    private Rigidbody2D _rb;
    private InputHandler _input;

    private Vector2 _lastDirection = Vector2.down;
    private bool _dashQueued;
    private bool _isDashing;
    private float _dashTimer;
    private float _cooldownTimer;
    private Vector2 _dashDirection;

    public bool IsDashing => _isDashing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<InputHandler>();

        // Top-down: no gravity, no spinning
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }
    

    private void Update()
    {
        // Read button presses in Update so they never get missed between physics ticks
        if (_input.takingInput && _input.DashAction.WasPressedThisFrame())
            _dashQueued = true;

        _cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Debug.Log($"move: {_input.MoveVector} | vel: {_rb.linearVelocity} | taking: {_input.takingInput} | bodyType: {_rb.bodyType}");
        
        if (_isDashing)
        {
            HandleDash();
            return;
        }

        Vector2 dir = _input.MoveVector;
        if (dir.sqrMagnitude > 1f) dir.Normalize(); // stops diagonals being faster
        if (dir != Vector2.zero) _lastDirection = dir.normalized;

        if (_dashQueued && _cooldownTimer <= 0f)
            StartDash(dir != Vector2.zero ? dir.normalized : _lastDirection);
        _dashQueued = false;

        float topSpeed = _input.IsSprinting ? sprintSpeed : walkSpeed;
        Vector2 targetVelocity = dir * topSpeed;

        // Accelerate when pushing a direction, decelerate when letting go
        float rate = dir != Vector2.zero ? acceleration : deceleration;
        _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    }

    private void StartDash(Vector2 direction)
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashDirection = direction;
        _cooldownTimer = dashCooldown;
    }

    private void HandleDash()
    {
        _rb.linearVelocity = _dashDirection * dashSpeed;
        _dashTimer -= Time.fixedDeltaTime;

        if (_dashTimer <= 0f)
            _isDashing = false;
    }
}