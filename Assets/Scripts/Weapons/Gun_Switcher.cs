using UnityEngine;
using UnityEngine.InputSystem;

public class Gun_Switcher : MonoBehaviour
{
    [SerializeField] private GameObject[] guns;
    private int currentIndex = 0;

    private InputAction _switchGunAction;
    
    private void Awake()
    {
        _switchGunAction = InputSystem.actions.FindAction("SwitchGun");
    }

    private void Start()
    {
        SetActiveGun(currentIndex);
    }

    private void Update()
    {
        if (_switchGunAction.WasPerformedThisFrame())
        {
            currentIndex = (currentIndex + 1) % guns.Length;
            SetActiveGun(currentIndex);
        }
    }

    private void SetActiveGun(int index)
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(i == index);
        }
    }
}