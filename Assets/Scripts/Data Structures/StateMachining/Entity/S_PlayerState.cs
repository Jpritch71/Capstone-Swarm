﻿using System;

/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// PLAYER STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
public abstract class S_PlayerState : I_State
{
    protected PlayerEntityController ownerPlayer;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_PlayerState(PlayerEntityController playerIn)
    {
        ownerPlayer = playerIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        ownerPlayer.C_StateMachine.SetCurrentState(stateIn);
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
        if (!ownerPlayer.C_Movement.Moving)
        {
            SetControllerState(new S_Player_Idle(ownerPlayer));
        }
    }

    public override void OnStart()
    {
        ownerPlayer.AnimController.StartRunning();
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
        if (ownerPlayer.C_Movement.Moving)
        {
            SetControllerState(new S_Player_Running(ownerPlayer));
        }
    }

    public override void OnStart()
    {
        ownerPlayer.AnimController.StartIdle();
    }

    public override void OnStopped()
    {

    }
}