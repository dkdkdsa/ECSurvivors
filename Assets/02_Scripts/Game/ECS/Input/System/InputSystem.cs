using Unity.Entities;
using UnityEngine;

public partial class InputSystem : SystemBase
{
    private PlayerInputActions _actions;

    protected override void OnCreate()
    {
        EntityManager.CreateSingleton<InputData>();
        _actions = new PlayerInputActions();
        _actions.Enable();
    }

    protected override void OnDestroy()
    {
        _actions.Dispose();
    }

    protected override void OnUpdate()
    {
        SystemAPI.SetSingleton(new InputData
        {
            Move = _actions.Player.Move.ReadValue<Vector2>(),
        });
    }
}