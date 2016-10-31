using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class SquadController : GroupManager
{
    [SerializeField]
    private bool _AttackMode = false;
    public bool AttackMode { get { return _AttackMode; } set { _AttackMode = value; } }
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    protected HashSet<I_Entity> TargetsToHunt;

    // Use this for initialization
    protected override void InitAwake()
    {
        base.InitAwake();
        C_Movement = GetComponent<A_GridMover>();
        C_GroupManager = GetComponent<GroupManager>();

        TargetsToHunt = new HashSet<I_Entity>();

        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<SquadVision>();
        C_Vision = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<SquadVision>();
    }

    protected override void InitStart()
    {
        base.InitStart();
        C_GridMovement.BaseSpeed = 9f;
        C_Vision.VisionRange = 10f;
        //LoadStats();
    }

    public void Start_NormalMovement()
    {
        AttackMode = false;
        foreach (GroupUnitMovement unit in C_Squad_List)
        {
            unit.StartGroupMovement();
        }
    }

    public void Start_AttackMovement()
    {
        AttackMode = true;
        foreach (GroupUnitMovement unit in C_Squad_List)
        {
            unit.StartMovingToTarget();
        }
    }

    public void AddTargetToHunt(I_Entity entityIn)
    {

    }

    public void MoveToEntity(I_Entity entityIn)
    {

    }

    public void ClearTarget()
    {
        Start_NormalMovement();
    }

    //public abstract void LoadStats();
    #region Components
    public I_Movement C_Movement
    {
        get
        {
            return C_GridMovement;
        }
        protected set
        {
            C_GridMovement = (A_GridMover)value;
        }
    }
    public A_GridMover C_GridMovement { get; private set; }

    public GroupManager C_GroupManager { get; protected set; }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }

    public I_Vision C_Vision { get; protected set; }    
    #endregion
}

