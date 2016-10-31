using UnityEngine;
using System.Collections;
using System;

public class PlayerEntityController : GroupUnitController
{
    public override int EntityLayer { get { return 11; } }

    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    // Use this for initialization
    protected override void InitAwake()
    {
        C_GroupManager = transform.parent.parent.GetComponent<PlayerGroupManager>();
        C_Movement = GetComponent<GroupUnitMovement>();
        C_Movement.C_Controller = this;       
        AnimController = new PlayerAnimController(GameObject.Find("PlayerUnit").GetComponentInChildren<Animation>());
        C_StateMachine = new StateMachine(null, new S_Player_Idle(this));

        C_SquadController = C_AttachedGameObject.transform.parent.parent.GetComponent<SquadController>();
    }

    protected override void InitStart()
    {
        LoadStats();
    }

    // Update is called once per frame
    void Update()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine

        if (Input.GetKeyDown(KeyCode.A))
            return;

        if (Input.GetMouseButton(0) && C_PlayerGridMovement.CanMove)
        {
            if (!C_PlayerGridMovement.DirectionalMovement)
            {
                C_PlayerGridMovement.StopMoving();
            }
            C_PlayerGridMovement.StartFreeMoving();
        }
        else if (Input.GetMouseButtonUp(0) || !C_PlayerGridMovement.CanMove)
        {
            C_PlayerGridMovement.EndFreeMove();
        }
        if (Input.GetMouseButtonDown(1) && !C_PlayerGridMovement.DirectionalMovement)
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
                C_PlayerGridMovement.SetPathToPoint(hit.point);
            }
        }
    }

    public override void LoadStats()
    {
        print("GET THE PLAYER STATS");
        C_PlayerGridMovement.BaseSpeed = 10f;
        C_Entity = new PlayerEntity(this, 1000f, 1);
    }

    #region Components
    public PlayerMovement C_PlayerGridMovement
    {
        get
        {
            return (PlayerMovement)C_GroupManager.GroupGridComponent;
        }
    }
    public PlayerAnimController AnimController { get; protected set; }
    public PlayerGroupManager C_GroupManager { get; protected set; }
    #endregion
}