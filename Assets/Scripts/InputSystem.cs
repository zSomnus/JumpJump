using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    InputControls controls;
    [SerializeField] Player player;

    void Awake()
    {
        controls = new InputControls();
        controls.Player.MeleeAttack.performed += ctx => player.MeleeAttack();
        controls.Player.RangedAttack.performed += ctx => player.RangedAttack();
        controls.Player.Move.performed += ctx => player.Direction = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => player.Direction = Vector2.zero;
        controls.Player.Jump.performed += ctx => player.Jump();
        controls.Player.Jump.performed += ctx => player.DoubleJump();
        controls.Player.Jump.performed += ctx => player.WallJump();
        controls.Player.Jump.performed += ctx => player.IsJumpPressed = true;
        controls.Player.Jump.canceled += ctx => player.IsJumpPressed = false;
        controls.Player.Dash.performed += ctx => player.Dash();
        controls.Player.Dash.performed += ctx => player.IsDashPressed = true;
        controls.Player.Dash.canceled += ctx => player.IsDashPressed = false;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
}
