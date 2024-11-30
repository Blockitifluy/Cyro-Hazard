using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerBehaviour : CharacterControl
{
    private InputActionMap _InputActionMap;
    private InputAction _MovementAction;

    public InputActionAsset Controls;

    private void ControlMovementOnInput()
    {
        Vector2 dir = _MovementAction.ReadValue<Vector2>();

        movementBasics.UpdateForwardsDir(dir.y);
        movementBasics.UpdateTurning(dir.x);
    }

    public void Start()
    {
        _InputActionMap = Controls.FindActionMap("gameplay");

        _MovementAction = _InputActionMap.FindAction("movement");
    }

    protected override void Update()
    {
        ControlMovementOnInput();
    }
}
