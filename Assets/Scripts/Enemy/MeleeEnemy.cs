using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Melee")]
    [Tooltip("Stops and starts attacking when this close.")]
    [SerializeField] private float _attackRange = 1.2f;
    [Tooltip("Player must still be within this distance when the swing lands.")]
    [SerializeField] private float _hitRange = 1.6f;
    [SerializeField] private float _windup = 0.4f;
    [SerializeField] private float _recovery = 0.6f;
    [SerializeField] private Color _windupColor = Color.red;

    // SFX
    [Header("Melee Sounds")]
    [Tooltip("Plays when it starts winding up (the telegraph).")]
    [SerializeField] private SoundEffect _windupSound = new SoundEffect();
    [Tooltip("Plays when the swing happens, hit or miss. The player's own hurt sound covers a hit.")]
    [SerializeField] private SoundEffect _swingSound = new SoundEffect();

    private enum State { Chasing, Windup, Recovery }
    private State _state = State.Chasing;
    private float _stateTimer;

    protected override void Awake()
    {
        base.Awake();
        _damageOnContact = false; // damage only comes from the swing, never from walking into the player
    }

    protected override void Behave()
    {
        switch (_state)
        {
            case State.Chasing:
                if (DistanceToPlayer <= _attackRange)
                {
                    Stop();
                    EnterState(State.Windup, _windup);
                    Tint(_windupColor);
                    SoundManager.Play(_windupSound); // SFX
                }
                else
                {
                    _rb.linearVelocity = DirectionToPlayer * _speed;
                }
                break;

            case State.Windup:
                Stop();
                _stateTimer -= Time.fixedDeltaTime;
                if (_stateTimer <= 0f)
                {
                    SoundManager.Play(_swingSound); // SFX
                   
                    if (DistanceToPlayer <= _hitRange)
                        DamagePlayer(_damage);

                    ResetTint();
                    EnterState(State.Recovery, _recovery);
                }
                break;

            case State.Recovery:
                Stop();
                _stateTimer -= Time.fixedDeltaTime;
                if (_stateTimer <= 0f)
                    _state = State.Chasing; 
                break;
        }
    }

    private void EnterState(State state, float duration)
    {
        _state = state;
        _stateTimer = duration;
    }
}