using System;
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Ork STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
public abstract class S_OrkState : I_State
{
    protected OrkUnitController ownerOrk;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_OrkState(OrkUnitController OrkIn)
    {
        ownerOrk = OrkIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        ownerOrk.stateController.SetCurrentState(stateIn);
    }

    public abstract void Execute();
    public abstract void OnPaused();
    public abstract void OnStart();
    public abstract void OnStopped();
}

public class S_Ork_Running : S_OrkState
{
    public S_Ork_Running(OrkUnitController OrkIn) : base(OrkIn)
    {

    }

    public override void Execute()
    {
        if (!ownerOrk.MovementComponent.Moving)
        {
            SetControllerState(new S_Ork_Idle(ownerOrk));
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
    }

    public override void OnStart()
    {
        ownerOrk.AnimController.StartRunning();
    }

    public override void OnStopped()
    {

    }
}

public class S_Ork_Idle : S_OrkState
{
    public S_Ork_Idle(OrkUnitController OrkIn) : base(OrkIn)
    {

    }

    public override void Execute()
    {
        if (ownerOrk.MovementComponent.Moving)
        {
            SetControllerState(new S_Ork_Running(ownerOrk));
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
    }

    public override void OnStart()
    {
        ownerOrk.AnimController.StartIdle();
    }

    public override void OnStopped()
    {

    }
}

public class S_Ork_Attack : S_OrkState
{
    public S_Ork_Attack(OrkUnitController OrkIn) : base(OrkIn)
    {

    }

    public override void Execute()
    {
        if (ownerOrk.MovementComponent.Moving)
        {
            SetControllerState(new S_Ork_Running(ownerOrk));
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
    }

    public override void OnStart()
    {
        ownerOrk.AnimController.StartIdle();
    }

    public override void OnStopped()
    {

    }
}