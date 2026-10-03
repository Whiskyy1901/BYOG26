using UnityEngine;
 
public class BasicEnemy : Enemy
{
    protected override void Behave()
    {
        // Walk straight at the player
        _rb.linearVelocity = DirectionToPlayer * _speed;
    }
}