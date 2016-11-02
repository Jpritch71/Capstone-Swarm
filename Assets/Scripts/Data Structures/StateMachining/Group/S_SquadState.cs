using System;
using UnityEngine;
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Ork STATES
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
public abstract class S_SquadState : I_State
{
    protected SquadController squad;

    public ID_Status currentStatus
    {
        get; protected set;
    }

    public S_SquadState(SquadController squadIn)
    {
        squad = squadIn;
    }

    protected void SetControllerState(I_State stateIn)
    {
        squad.C_StateMachine.SetCurrentState(stateIn);
    }

    protected void StartPreviousState()
    {
        squad.C_StateMachine.StartPreviousState();
    }

    protected void ExecuteBlip(A_Blip blipIn)
    {
        squad.C_StateMachine.GoIntoBlip(blipIn);
    }

    public abstract void Execute();
    public abstract void OnStart();
    public abstract void OnPause();
    public abstract void OnStopped();
    public abstract void OnResume();
}

/// <summary>
/// Idle (not moving) state-command for generic ork squads
/// Squads in this order have no active orders, and are receptive to any new orders.
/// Avoid directly calling behavior methods on a squad; let Order-States handle these actions.
/// Transitions: Attack||Retaliation-Command --> S_Squad_HuntAndKill || S_Squad_Retaliate
/// Transitions: Move-Command --> S_Squad_OrderMove
/// </summary>
public class S_Squad_Idle : S_SquadState
{
    public S_Squad_Idle(SquadController squadIn) : base(squadIn)
    {
    }

    public override void Execute()
    {
        if (squad.CurrentOrder != null && !squad.CurrentOrder.OrderStarted)
        {
            if (squad.CurrentOrder.TypeOfOrder == A_SquadOrder.OrderType.Movement)
            {
                SetControllerState(new S_Squad_OrderMove(squad, squad.CurrentOrder));
                return;
            }
            if (squad.CurrentOrder.TypeOfOrder == A_SquadOrder.OrderType.SearchAndDestroy)
            {
                SetControllerState(new S_Squad_SearchAndDestroy(squad, squad.CurrentOrder));
                return;
            }
            if (squad.CurrentOrder.TypeOfOrder == A_SquadOrder.OrderType.Retaliation)
            {
                SetControllerState(new S_Squad_Retaliate(squad, squad.CurrentOrder));
                return;
            }
        }
        else
        {
            if (squad.C_AggresionSphere.TargetAcquired)
            {
                Debug.Log("Add Attack order");
                squad.AddOrder(new Order_SquadAttack(squad, false, squad.C_AggresionSphere.Target));
            }
        }
    }

    public override void OnStart()
    {
        squad.Start_NormalMovement();
    }
 
    public override void OnStopped()
    {
    }

    public override void OnResume()
    {
    }

    public override void OnPause()
    {
    }
}

public abstract class S_SquadOrderState : S_SquadState
{
    protected A_SquadOrder order;
    public S_SquadOrderState(SquadController squadIn, A_SquadOrder orderIn) : base(squadIn)
    {
        order = orderIn;
    }

    public override void Execute()
    {
        if(order.OrderCancelled || order.OrderSatisfied())
        {
            if(order.OrderCancelled)
            {
                int x = 0;
            }
            SetControllerState(new S_Squad_Idle(squad));
            return;
        }
        if(squad.OrderWaiting && order.Interruptible)
        {
            order.CancelOrder();
        }
        OrderUpdate();
    }

    protected abstract void OrderUpdate();

    public override void OnStart()
    {
        order.OrderAction();
    }

    public override void OnStopped()
    {
        order.CompleteAction();
    }

    public override void OnResume()
    {
    }

    public override void OnPause()
    {
    }
}

/// <summary>
/// Command Move state-command for generic ork squads
/// Transitions: Idle-Command --> S_Squad_CommmandIdle, Attack-Command --> S_Squad_CommandAttack
/// </summary>
public class S_Squad_OrderMove : S_SquadOrderState
{
    public S_Squad_OrderMove(SquadController squadIn, A_SquadOrder orderIn) : base(squadIn, orderIn)
    {
    }

    protected override void OrderUpdate()
    {
        if(order.OrderSatisfied())
        {
            SetControllerState(new S_Squad_Idle(squad));
            return;
        }
    }
}

/// <summary>
/// Idle (not moving) state-command for generic ork squads
/// Transitions: Moving-Command --> S_Squad_CommmandMoving
/// </summary>
public class S_Squad_Retaliate : S_SquadOrderState
{
    protected Order_SquadAttack attackOrder { get { return (Order_SquadAttack)order; } }
    public S_Squad_Retaliate(SquadController squadIn, A_SquadOrder orderIn) : base(squadIn, orderIn)
    {
    }

    protected override void OrderUpdate()
    {
        if (order.OrderSatisfied() || squad.OrderWaiting)
        {
            SetControllerState(new S_Squad_Idle(squad));
            return;
        }
        if (squad.C_AggresionSphere.TargetAcquired && attackOrder.TargetEntity == squad.C_AggresionSphere.Target)
        {
            if(!squad.AttackMode)
            {
                ExecuteBlip(new Blip_Squad_UnitsAttack(squad));
            }
        }
        else
        {
            SetControllerState(new S_Squad_Idle(squad));
        }
    }
}

/// <summary>
/// Idle (not moving) state-command for generic ork squads
/// Transitions: Moving-Command --> S_Squad_CommmandMoving
/// </summary>
public class S_Squad_SearchAndDestroy : S_SquadOrderState
{
    public S_Squad_SearchAndDestroy(SquadController squadIn, A_SquadOrder orderIn) : base(squadIn, orderIn)
    {
    }

    protected override void OrderUpdate()
    {
        if (squad.C_AggresionSphere.TargetAcquired)
        {
            if (!squad.AttackMode)
            {
                ExecuteBlip(new Blip_Squad_UnitsAttack(squad));
            }
        }
        else
        {
            if (squad.AttackMode)
            {
                ExecuteBlip(new Blip_Squad_UnitsCeaseAttack(squad));
            }
        }
    }
}


/////////////////////////////////////////////////SQUAD BLIPS///////////////////////////////////////////////////////////////////////
///Blips are pseudo states meant to change behavior within a state but are not entirely their own state                          ///
/////////////////////////////////////////////////SQUAD BLIPS///////////////////////////////////////////////////////////////////////

public class Blip_Squad_UnitsAttack : A_Blip
{
    private SquadController squad;

    public Blip_Squad_UnitsAttack(SquadController squadIn) : base(squadIn.C_StateMachine)
    {
        squad = squadIn;
    }

    protected override void BlipAction()
    {
        squad.Start_AttackMovement();
    }
}

public class Blip_Squad_UnitsCeaseAttack : A_Blip
{
    private SquadController squad;

    public Blip_Squad_UnitsCeaseAttack(SquadController squadIn) : base(squadIn.C_StateMachine)
    {
        squad = squadIn;
    }

    protected override void BlipAction()
    {
        squad.Start_NormalMovement();
    }
}