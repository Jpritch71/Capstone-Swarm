/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Ork STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
public abstract class S_SquadState : I_State
{
    protected SquadController workingSquad;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_SquadState(SquadController squadIn)
    {
        workingSquad = squadIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        workingSquad.C_StateMachine.SetCurrentState(stateIn);
    }

    protected void ReturnToPreviousState()
    {
        workingSquad.C_StateMachine.ReturnToPreviousState();
    }

    public abstract void Execute();
    public abstract void OnStart();
    public abstract void OnStopped();
}

/// <summary>
/// Idle (not moving) state-command for generic ork squads
/// Transitions: Attack-Command --> S_Squad_CommmandAttack
/// </summary>
public class S_Squad_CommandIdle : S_SquadState
{
    public S_Squad_CommandIdle(SquadController squadIn) : base(squadIn)
    {
    }

    public override void Execute()
    {
        if (workingSquad.C_Vision.TargetAcquired)
        {
            SetControllerState(new S_Squad_CommandAttack(workingSquad));
        }
    }

    public override void OnStart()
    {
        workingSquad.Start_NormalMovement();
    }

    public override void OnStopped()
    {

    }
}

/// <summary>
/// Idle (not moving) state-command for generic ork squads
/// Transitions: Moving-Command --> S_Squad_CommmandMoving
/// </summary>
public class S_Squad_CommandAttack : S_SquadState
{
    public S_Squad_CommandAttack(SquadController squadIn) : base(squadIn)
    {
    }

    public override void Execute()
    {
        if (!workingSquad.C_Vision.TargetAcquired)
        {
            SetControllerState(new S_Squad_CommandIdle(workingSquad));
        }
    }

    public override void OnStart()
    {
        workingSquad.Start_AttackMovement();
    }

    public override void OnStopped()
    {
        workingSquad.Start_NormalMovement();
    }
}