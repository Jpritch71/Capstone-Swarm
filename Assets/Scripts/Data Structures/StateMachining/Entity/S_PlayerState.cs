using System;
using UnityEngine;
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// PLAYER STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
public abstract class S_PlayerState : I_State
{
    protected PlayerEntityController player;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_PlayerState(PlayerEntityController playerIn)
    {
        player = playerIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        player.C_StateMachine.SetCurrentState(stateIn);
    }

    protected void StartPreviousState()
    {
        player.C_StateMachine.StartPreviousState();
    }

    protected void ExecuteBlip(A_Blip blipIn)
    {
        player.C_StateMachine.GoIntoBlip(blipIn);
    }

    public abstract void Execute();
    public abstract void OnStart();
    public abstract void OnStopped();

    public void OnResume()
    {
    }
    public void OnPause()
    {
    }
}

public class S_Player_Running : S_PlayerState
{
    public S_Player_Running(PlayerEntityController playerIn) : base(playerIn)
    {

    }

    public override void Execute()
    {
        if(player.C_Entity.WeaponManager.ActiveWeapon.Attacking)
        {
            SetControllerState(new S_Player_Attack(player));
            return;
        }
        if (!player.C_Movement.Moving)
        {
            SetControllerState(new S_Player_Idle(player));
        }
    }

    public override void OnStart()
    {
        player.AnimController.StartRunning();
    }

    public override void OnStopped()
    {

    }
}

public class S_Player_Idle : S_PlayerState
{
    public S_Player_Idle(PlayerEntityController playerIn) : base(playerIn)
    {

    }

    public override void Execute()
    {
        if (player.C_Entity.WeaponManager.ActiveWeapon.Attacking)
        {
            SetControllerState(new S_Player_Attack(player));
            return;
        }
        if (player.C_Movement.Moving)
        {
            SetControllerState(new S_Player_Running(player));
        }
    }

    public override void OnStart()
    {
        player.AnimController.StartIdle();
    }

    public override void OnStopped()
    {

    }
}

public class S_Player_Attack : S_PlayerState
{
    public S_Player_Attack(PlayerEntityController playerIn) : base(playerIn)
    {

    }

    public override void Execute()
    {
        if (player.C_Entity.WeaponManager.ActiveWeapon.ActiveAttack.AttackCompleted)
        {
            StartPreviousState();
        }
    }

    public override void OnStart()
    {
        player.AnimController.StartAttack();
        player.C_PlayerGridMovement.DisallowMovement();
    }

    public override void OnStopped()
    {
        player.C_PlayerGridMovement.AllowMovement();
    }
}