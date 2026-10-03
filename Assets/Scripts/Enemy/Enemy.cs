using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float _speed = 3f;
    [SerializeField] protected float _damage = 1f;

    [Header("Contact Attack")]
    [SerializeField] protected bool _damageOnContact = true;
    [SerializeField] protected float _timeBetweenAttacks = 1f;

    protected GameObject _player;
    protected Rigidbody2D _rb;
    protected SpriteRenderer _sprite;

    private Color _baseColor = Color.white;
    private float _attackTimer;

    // Same signature as before, so the spawner doesn't need to change
    public virtual void Initialization(GameObject player)
    {
        _player = player;
    }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        if (_sprite != null) _baseColor = _sprite.color;
        _attackTimer = _timeBetweenAttacks; // can hit immediately on first contact
    }

    protected virtual void Start()
    {
        // The spawner should call Initialization(), but if it didn't, find the player ourselves
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null)
                Debug.LogWarning($"{name}: no player assigned and nothing tagged 'Player' found, so it can't move.", this);
        }
    }

    protected virtual void Update()
    {
        _attackTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // Player not assigned yet, or died and was destroyed
        if (_player == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Behave();
    }

    /// <summary>Called every physics tick while the player exists. Put movement and AI here.</summary>
    protected abstract void Behave();

    /// <summary>Runs right after a contact hit lands. Override for things like exploding.</summary>
    protected virtual void OnContactAttack() { }

    protected Vector2 DirectionToPlayer =>
        ((Vector2)_player.transform.position - (Vector2)transform.position).normalized;

    protected float DistanceToPlayer =>
        Vector2.Distance(_player.transform.position, transform.position);

    /// <summary>Damages the player directly (for attacks that aren't plain contact).</summary>
    protected void DamagePlayer(float amount)
    {
        if (_player != null && _player.transform.root.TryGetComponent<Health>(out Health health))
            health.Damage(amount);
    }

    /// <summary>Cheap telegraph: tint the sprite. Call ResetTint() to go back to normal.</summary>
    protected void Tint(Color color)
    {
        if (_sprite != null) _sprite.color = color;
    }

    protected void ResetTint()
    {
        if (_sprite != null) _sprite.color = _baseColor;
    }

    protected void Stop() => _rb.linearVelocity = Vector2.zero;

    // Stay (not Enter) so an enemy standing on the player keeps hitting on cooldown
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_damageOnContact || _attackTimer < _timeBetweenAttacks) return;
        if (!other.CompareTag("Player")) return;

        if (other.transform.root.TryGetComponent<Health>(out Health health))
        {
            health.Damage(_damage);
            _attackTimer = 0f;
            OnContactAttack();
        }
    }
}