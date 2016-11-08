using UnityEngine;
using System.Collections;
using System;

public class OrkSquadController : SquadController
{
    public override int GroupLayer { get { return 12; } }
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    protected override void InitStart()
    {
        base.InitStart();
        C_StateMachine = new StateMachine(null, new S_Squad_Idle(this));
    }

    protected override void GroupLogic()
    {
        base.GroupLogic();
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine

        //if (Input.GetMouseButton(0) && C_Movement.CanMove)
        //{

        //}
        //else if (Input.GetMouseButtonUp(0) || !C_Movement.CanMove)
        //{

        //}
        //if (Input.GetMouseButtonDown(1) && C_Movement.CanMove)
        //{
        //    hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, Mathf.Infinity, (1 << 8) + (1 << 9));

        //    if (hits.Length > 0)
        //    {
        //        hit = hits[0];
        //        for (int x = 1; x < hits.Length; x++)
        //        {
        //            if (hit.point.y < hits[x].point.y)
        //                hit = hits[x];
        //        }
        //        //marker.transform.position = hit.point;
        //        AddOrder(new Order_SqaudMovement(this, false, hit.point));
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.Home))
        {
            I_Entity en = null;
            if(EntityManager.GetEntityByUniqueID(PlayerEntity.PlayerID, ref en))
                AddOrder(new Order_SquadSearchAndDestroy(this, true, (TaggableEntity)en));
        }
    }

    #region Components
    public GrouchoAnimController AnimController { get; private set; }
    #endregion
}
