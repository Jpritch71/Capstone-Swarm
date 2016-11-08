using UnityEngine;
using System.Collections;
using System;

public class Order_SqaudMovement : A_SquadOrder
{
    private Node orderedPosition;

    public Order_SqaudMovement(SquadController squadIn, bool interruptIn, Node posIn) : base(OrderType.Movement, squadIn, interruptIn)
    {
        orderedPosition = posIn;
    }

    public Order_SqaudMovement(SquadController squadIn, bool interruptIn, Vector3 posIn) : base(OrderType.Movement, squadIn, interruptIn)
    {
        orderedPosition = WorldManager._WORLD.GetTile(posIn);
    }

    protected override void DoAction()
    {
        squad.MoveToPosition(orderedPosition);
        squad.Start_NormalMovement();
        //Debug.Log(ToString() + " Acction Execute.");
    }

    protected override bool DoComplete()
    {
        squad.C_GridMovement.PathObj.impossiblePath = false;
        return OrderSatisfied();
    }

    public override bool OrderSatisfied()
    {
    //    var a = orderedPosition;
    //    var b = squad.C_GridMovement.Pos;

    //    Debug.Log("Ordered Position{" + a + "} || Grid Position{" + b + "}");
        return squad.C_GridMovement.isCharacterAtPointOffset(orderedPosition.Pos);
    }

    public override bool OrderValidation()
    {
        return orderedPosition.Walkable;
    }

    public override string ToString()
    {
        return "Order: " + TypeOfOrder + " --> " + orderedPosition;
    }
}
