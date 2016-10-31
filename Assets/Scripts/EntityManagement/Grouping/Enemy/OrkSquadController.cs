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
        C_StateMachine = new StateMachine(null, new S_Squad_CommandIdle(this));
    }

    protected override void GroupLogic()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine

        if (Input.GetMouseButton(0) && C_Movement.CanMove)
        {

        }
        else if (Input.GetMouseButtonUp(0) || !C_Movement.CanMove)
        {

        }
        if (Input.GetMouseButtonDown(1) && C_Movement.CanMove)
        {
            hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, Mathf.Infinity, (1 << 8) + (1 << 9));

            if (hits.Length > 0)
            {
                hit = hits[0];
                for (int x = 1; x < hits.Length; x++)
                {
                    if (hit.point.y < hits[x].point.y)
                        hit = hits[x];
                }
                //marker.transform.position = hit.point;
                C_GridMovement.SetPathToPoint(hit.point);
            }
        }
    }

    #region Components
    public GrouchoAnimController AnimController { get; private set; }
    #endregion
}
