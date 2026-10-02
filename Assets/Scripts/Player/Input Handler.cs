using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset _playerAction;

    private InputAction _moveAction;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
    }
}
