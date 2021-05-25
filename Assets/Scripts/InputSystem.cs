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
        if (!player.gameObject.activeInHierarchy)
        {
            return;
        }

        controls = new InputControls();
        controls.Player.MeleeAttack.performed += ctx => MeleeAttack();
        controls.Player.RangedAttack.performed += ctx => RangedAttack();
        controls.Player.Move.performed += ctx => GetDirection(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => GetDirection(Vector2.zero);
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Jump.performed += ctx => DoubleJump();
        controls.Player.Jump.performed += ctx => WallJump();
        controls.Player.Jump.performed += ctx => JumpPressed(true);
        controls.Player.Jump.canceled += ctx => JumpPressed(false);
        controls.Player.Dash.performed += ctx => Dash();
        controls.Player.Dash.performed += ctx => DashPressed(true);
        controls.Player.Dash.canceled += ctx => DashPressed(false);
    }

    void MeleeAttack()
    {
        if (player.isActiveAndEnabled)
        {
            player.MeleeAttack();
        }
    }

    void RangedAttack()
    {
        if (player.isActiveAndEnabled)
        {
            player.RangedAttack();
        }
    }

    void GetDirection(Vector2 direction)
    {
        if (player.isActiveAndEnabled)
        {
            player.Direction = direction;
        }
    }

    void Jump()
    {
        if (player.isActiveAndEnabled)
        {
            player.Jump();
        }
    }

    void DoubleJump()
    {
        if (player.isActiveAndEnabled)
        {
            player.DoubleJump();
        }
    }

    void WallJump()
    {
        if (player.isActiveAndEnabled)
        {
            player.WallJump();
        }
    }

    void JumpPressed(bool state)
    {
        if (player.isActiveAndEnabled)
        {
            player.IsJumpPressed = state;
        }
    }

    void Dash()
    {
        if (player.isActiveAndEnabled)
        {
            player.Dash();
        }
    }

    void DashPressed(bool state)
    {
        if (player.isActiveAndEnabled)
        {
            player.IsDashPressed = state;
        }
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
