using UnityEngine;
using System.Collections;

public abstract class A_SquadOrder
{
    public enum OrderType
    {
        Movement, SearchAndDestroy, Retaliation
    }

    public OrderType TypeOfOrder { get; protected set; }

    public bool OrderStarted { get; protected set; }
    public bool OrderCompleted { get; protected set; }
    public bool OrderCancelled { get; protected set; }
    public bool Interruptible { get; protected set; }

    protected SquadController squad;

    public A_SquadOrder(OrderType typeIn, SquadController squadIn, bool interruptIn)
    {
        TypeOfOrder = typeIn;
        squad = squadIn;
        Interruptible = interruptIn;

        OrderCompleted = false;
    }

    public abstract bool OrderValidation();

    public void OrderAction()
    {
        DoAction();
        OrderStarted = true;
    }

    protected abstract void DoAction();

    public void CompleteAction()
    {
        squad.OrderExited(DoComplete());
        OrderCompleted = true;
    }

    protected abstract bool DoComplete();

    public void CancelOrder()
    {       
        squad.OrderExited(DoComplete());
        OrderCancelled = true;
    }

    public abstract bool OrderSatisfied();
}

