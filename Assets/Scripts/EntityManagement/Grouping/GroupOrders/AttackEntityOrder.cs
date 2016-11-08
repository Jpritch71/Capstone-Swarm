using UnityEngine;
using System.Collections;
using System;

public class Order_SquadAttack : A_SquadOrder
{
    public TaggableEntity TargetEntity { get; protected set; }

    public Order_SquadAttack(SquadController squadIn, TaggableEntity targetEntityIn) : base(OrderType.Retaliation, squadIn, true)
    {
        TargetEntity = targetEntityIn;
    }

    public Order_SquadAttack(SquadController squadIn, bool interruptIn, TaggableEntity targetEntityIn) : base(OrderType.SearchAndDestroy, squadIn, interruptIn)
    {
        TargetEntity = targetEntityIn;
    }

    protected override void DoAction()
    {
        squad.MoveToEntity(TargetEntity);
        squad.Start_NormalMovement();
    }

    protected override bool DoComplete()
    {
        squad.Start_NormalMovement();
        return OrderSatisfied();
    }

    public override bool OrderSatisfied()
    {
        return TargetEntity.Dead;
    }

    public override bool OrderValidation()
    {
        return (TargetEntity != null && !TargetEntity.Dead);
    }

    public override string ToString()
    {
        return "Order: " + TypeOfOrder + " --> " + TargetEntity;
    }
}

public class Order_SquadSearchAndDestroy : Order_SquadAttack
{
    public Order_SquadSearchAndDestroy(SquadController squadIn, bool interruptIn, TaggableEntity targetEntityIn) : base(squadIn, interruptIn, targetEntityIn)
    {
    }

    protected override void DoAction()
    {
        squad.StartHuntingTarget(TargetEntity);
        squad.Start_NormalMovement();
    }

    public override bool OrderSatisfied()
    {
        return TargetEntity.Dead;
    }

    public override bool OrderValidation()
    {
        return (TargetEntity != null && !TargetEntity.Dead);
    }
}
