using System;
using UnityEngine;
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
        ownerOrk.C_StateMachine.SetCurrentState(stateIn);
    }

    protected void ReturnToPreviousState()
    {
        ownerOrk.C_StateMachine.ReturnToPreviousState();
    }

    public abstract void Execute();
    public abstract void OnPaused();
    public abstract void OnStart();
    public abstract void OnStopped();
}

/// <summary>
/// Running (moving) state for generic orks
/// Transitions: !Moving --> S_Ork_Idle
/// </summary>
public class S_Ork_Running : S_OrkState
{
    public S_Ork_Running(OrkUnitController OrkIn) : base(OrkIn)
    {

    }

    public override void Execute()
    {
        if (!ownerOrk.C_Movement.Moving)
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

/// <summary>
/// idle (not moving) state for generic orks
/// Transitions: Moving --> S_Ork_Running; 
/// </summary>
public class S_Ork_Idle : S_OrkState
{
    public S_Ork_Idle(OrkUnitController OrkIn) : base(OrkIn)
    {

    }

    public override void Execute()
    {
        if (ownerOrk.C_SquadController.C_Vision.TargetAcquired)
        {
            ownerOrk.C_Entity.C_MonoBehavior._MSG("Hunting");
            ownerOrk.C_Entity.C_MonoBehavior._MSG(Vector3.Distance(ownerOrk.C_UnitMovement.Pos, ownerOrk.C_SquadController.C_Vision.Target.Pos));
            if(Vector3.Distance(ownerOrk.C_UnitMovement.Pos, ownerOrk.C_SquadController.C_Vision.Target.Pos) < ownerOrk.C_Entity.AttackManager.AutoAttack.AttackingWeapon.WeaponRange)
            {
                SetControllerState(new S_Ork_Attack(ownerOrk));
                return;
            }
        }
        if (ownerOrk.C_Movement.Moving)
        {
            SetControllerState(new S_Ork_Running(ownerOrk));
            return;
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
    protected bool attacking;
    public S_Ork_Attack(OrkUnitController OrkIn) : base(OrkIn)
    {
    }

    public override void Execute()
    {
        if(ownerOrk.C_SquadController.C_GridMovement.Moving)
        {

        }
        if (ownerOrk.C_Entity.AttackManager.AutoAttack.AttackCompleted)
        {
            ReturnToPreviousState();
            return;
        }
    }

    public override void OnPaused()
    {
        throw new NotImplementedException();
    }

    public override void OnStart()
    {
        ownerOrk.AnimController.StartAttack();
        ownerOrk.C_Entity.AttackManager.AutoAttack.DoAttack();
    }

    public override void OnStopped()
    {
        
    }
}