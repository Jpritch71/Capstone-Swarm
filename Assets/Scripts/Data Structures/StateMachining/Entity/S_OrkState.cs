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

    protected void StartPreviousState()
    {
        ownerOrk.C_StateMachine.StartPreviousState();
    }

    protected void ExecuteBlip(A_Blip blipIn)
    {
        ownerOrk.C_StateMachine.GoIntoBlip(blipIn);
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
            return;
        }
        if (ownerOrk.C_Movement.AttackMode && !ownerOrk.C_Entity.WeaponManager.ActiveWeapon.Attacking)
        {
            if (ownerOrk.C_Movement.DistanceToTarget <= ownerOrk.C_Entity.WeaponManager.ActiveWeapon.WeaponRange)
                SetControllerState(new S_Ork_Attack(ownerOrk));
        }
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
        if (ownerOrk.C_Movement.Moving)
        {
            SetControllerState(new S_Ork_Running(ownerOrk));
            return;
        }
        if(ownerOrk.C_Movement.AttackMode && !ownerOrk.C_Entity.WeaponManager.ActiveWeapon.Attacking)
        {
            if (ownerOrk.C_Movement.DistanceToTarget <= ownerOrk.C_Entity.WeaponManager.ActiveWeapon.WeaponRange)
                SetControllerState(new S_Ork_Attack(ownerOrk));
        }
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
        if (ownerOrk.C_Entity.WeaponManager.ActiveWeapon.ActiveAttack.AttackCompleted)
        {
            //StartPreviousState();
            SetControllerState(new S_Ork_Idle(ownerOrk));
        }
    }

    public override void OnStart()
    {
        ownerOrk.AnimController.StartAttack();
        ownerOrk.C_Entity.WeaponManager.ActiveWeapon.DoAttack();
        ownerOrk.C_Movement.PauseMoving();
    }

    public override void OnStopped()
    {
        ownerOrk.C_Movement.StartMoving();
    }
}