using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public bool takingInput; // To disable input for stun or an enemy attack
    
    private InputAction _moveAction;
    private Vector2 _moveVector;
    private InputAction _dashAction;
    private InputAction _sprintAction;
    private bool _isSprinting;

    public Vector2 MoveVector => _moveVector;
    public InputAction DashAction => _dashAction;
    public bool IsSprinting => _isSprinting;


    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _dashAction = InputSystem.actions.FindAction("Dash");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    private void Update()
    {
        if (!takingInput)
            return;
        _moveVector = _moveAction.ReadValue<Vector2>();
        _isSprinting = _sprintAction.IsInProgress();
    }
}
