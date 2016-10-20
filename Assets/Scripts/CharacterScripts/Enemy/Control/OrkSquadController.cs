using UnityEngine;
using System.Collections;
using System;

public class OrkSquadController : SquadController
{
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    // Update is called once per frame
    void Update()
    {
        //if (stateController != null)
        //    stateController.ExecuteUpdate(); //update State Machine

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

    public override void LoadStats()
    {
        C_GridMovement.BaseSpeed = 9f;
    }

    #region Components
    public GrouchoAnimController AnimController { get; private set; }
    #endregion
}
