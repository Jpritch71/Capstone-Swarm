﻿//using UnityEngine;
//using System.Collections;
//using System;

//public class PlayerManager : PlayerGroupManager, I_Controller
//{
//    public int EntityLayer { get { return 11; } }

//    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
//    private RaycastHit hit;

//    // Use this for initialization
//    protected override void InitAwake ()
//    {
//        base.InitAwake();
//        C_Movement = GetComponent<PlayerMovement>();
//        C_UnitMovement = GameObject.Find("PlayerUnit").GetComponentInChildren<GroupUnitMovement>();
//        C_Movement.C_Owner_Component = this;
//        C_UnitMovement.C_Owner_Component = this;
//        C_PlayerGridMovement.BaseSpeed = 10f;
//        AnimController = new PlayerAnimController(GameObject.Find("PlayerUnit").GetComponentInChildren<Animation>());
//        //C_StateMachine = new StateMachine(null, new S_Player_Idle(this));
//        LoadStats();
//    }

//    // Update is called once per frame
//    void Update ()
//    {
//        UpdateGroup();

//        if (C_StateMachine != null)
//            C_StateMachine.ExecuteUpdate(); //update State Machine

//        if (Input.GetKeyDown(KeyCode.A))
//            return;

//        if (Input.GetMouseButton(0) && C_PlayerGridMovement.CanMove)
//        {
//            if (!C_PlayerGridMovement.DirectionalMovement)
//            {
//                C_Movement.StopMoving();
//            }
//            C_PlayerGridMovement.StartFreeMoving();
//        }
//        else if(Input.GetMouseButtonUp(0) || !C_PlayerGridMovement.CanMove)
//        {
//            C_PlayerGridMovement.EndFreeMove();
//        }
//        if (Input.GetMouseButtonDown(1) && !C_PlayerGridMovement.DirectionalMovement)
//        {
//            hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, Mathf.Infinity, (1 << 8) + (1 << 9));

//            if (hits.Length > 0)
//            {
//                hit = hits[0];
//                for (int x = 1; x < hits.Length; x++)
//                {
//                    if (hit.point.y < hits[x].point.y)
//                        hit = hits[x];
//                }
//                //marker.transform.position = hit.point;
//                C_PlayerGridMovement.SetPathToPoint(hit.point);
//            }
//        }
//    }

//    public void LoadStats()
//    {
//        print("GET THE PLAYER STATS");
//        C_Entity = new PlayerEntity(this, 200f, 1);
//    }

//    #region Components
//    public I_Movement C_Movement { get; protected set; }  
//    public PlayerMovement C_PlayerGridMovement
//    {
//        get
//        {
//            return (PlayerMovement)C_Movement;
//        }
//        protected set
//        {
//            C_Movement = value;
//        }
//    }
//    public GroupUnitMovement C_UnitMovement
//    {
//        get; protected set;
//    }
//    public PlayerAnimController AnimController { get; protected set; }
//    public StateMachine C_StateMachine { get; protected set; }

//    public I_Entity C_Entity
//    {
//        get;
//        protected set;
//    }
//    #endregion
//}
