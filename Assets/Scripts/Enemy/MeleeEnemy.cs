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