using System;


public enum ID_Status { started, paused, stopped }

public interface I_State
{
    ID_Status currentStatus { get; }

    void OnStart();
    void OnPaused();
    void Execute();
    void OnStopped();
}

public interface I_GlobalState : I_State
{
    bool IsStateValid();
}

/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// PLAYER STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>

public abstract class S_PlayerState : I_State
{
    protected PlayerController ownerPlayer;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_PlayerState(PlayerController playerIn)
    {
        ownerPlayer = playerIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        ownerPlayer.stateController.SetCurrentState(stateIn);
    }

    public abstract void Execute();
    public abstract void OnPaused();
    public abstract void OnStart();
    public abstract void OnStopped();
}

public class S_Player_Running : S_PlayerState
{
    public S_Player_Running(PlayerController playerIn) : base(playerIn)
    {

    }

    public override void Execute()
    {
        if (!ownerPlayer.MovementComponent.Moving)
        {
            SetControllerState(new S_Player_Idle(ownerPlayer));
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
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
    public S_Player_Idle(PlayerController playerIn) : base(playerIn)
    {

    }

    public override void Execute()
    {
        if (ownerPlayer.MovementComponent.Moving)
        {
            SetControllerState(new S_Player_Running(ownerPlayer));
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
    }

    public override void OnStart()
    {
        ownerPlayer.AnimController.StartIdle();
    }

    public override void OnStopped()
    {

    }
}


