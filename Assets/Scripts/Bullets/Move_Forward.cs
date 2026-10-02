using System;
using UnityEngine;

public class Move_Forward : MonoBehaviour
{
    private float _speed;
    
    public void Initialization(float speed)
    {
        _speed = speed;
    }

    private void Start()
    {
        Destroy(this.gameObject, 5);
    }

    private void FixedUpdate()
    {
        transform.position += transform.right * _speed * Time.deltaTime;
    }
}
