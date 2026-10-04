using UnityEngine;

public class DasherEnemy : Enemy
{
    [Header("Dash")]
    [Tooltip("Starts charging up when this close to the player.")]
    [SerializeField] private float _triggerRange = 6f;
    [SerializeField] private float _chargeTime = 1f;
    [SerializeField] private float _dashSpeed = 14f;
    [SerializeField] private float _dashDuration = 0.35f;
    [Tooltip("1 = single dash. Higher tiers chain more dashes per charge-up.")]
    [SerializeField] private int _dashesPerCharge = 1;
    [Tooltip("Short stop between chained dashes, where it re-aims at the player.")]
    [SerializeField] private float _pauseBetweenDashes = 0.25f;
    [SerializeField] private float _recoveryTime = 1f;
    [SerializeField] private Color _chargeColor = Color.yellow;

    // SFX
    [Header("Dasher Sounds")]
    [Tooltip("Plays once when it starts charging up (the telegraph).")]
    [SerializeField] private SoundEffect _chargeSound = new SoundEffect();
    [Tooltip("Plays at the start of every dash, including chained ones.")]
    [SerializeField] private SoundEffect _dashSound = new SoundEffect();

    private enum State { Chasing, Charging, Dashing, Pausing, Recovering }
    private State _state = State.Chasing;
    private float _stateTimer;
    private int _dashesLeft;
    private Vector2 _dashDirection;

    
    protected override void Behave()
    {
        switch (_state)
        {
            case State.Chasing:
                if (DistanceToPlayer <= _triggerRange)
                {
                    Stop();
                    EnterState(State.Charging, _chargeTime);
                    Tint(_chargeColor);
                    SoundManager.Play(_chargeSound); // SFX
                }
                else
                {
                    _rb.linearVelocity = DirectionToPlayer * _speed;
                }
                break;

            case State.Charging:
                Stop();
                if (TimerDone())
                {
                    _dashesLeft = _dashesPerCharge;
                    StartDash();
                }
                break;

            case State.Dashing:
                _rb.linearVelocity = _dashDirection * _dashSpeed;
                if (TimerDone())
                {
                    Stop();
                    if (_dashesLeft > 0)
                    {
                        EnterState(State.Pausing, _pauseBetweenDashes);
                    }
                    else
                    {
                        ResetTint();
                        EnterState(State.Recovering, _recoveryTime);
                    }
                }
                break;

            case State.Pausing:
                Stop();
                if (TimerDone())
                    StartDash(); // re-aims, so chained dashes track the player
                break;

            case State.Recovering:
                Stop();
                if (TimerDone())
                    _state = State.Chasing;
                break;
        }
    }

    private void StartDash()
    {
        _dashDirection = DirectionToPlayer; // locked for the whole dash, so it can be dodged
        _dashesLeft--;
        EnterState(State.Dashing, _dashDuration);
        SoundManager.Play(_dashSound); // SFX
    }

    private void EnterState(State state, float duration)
    {
        _state = state;
        _stateTimer = duration;
    }

    private bool TimerDone()
    {
        _stateTimer -= Time.fixedDeltaTime;
        return _stateTimer <= 0f;
    }
}