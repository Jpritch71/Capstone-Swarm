using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class SquadController : GroupManager
{
    public A_SquadOrder CurrentOrder { get; protected set; }
    public bool HuntOrder { get; protected set; }
    public bool OrderWaiting { get { return orders.Count > 0; } }
    protected Queue<A_SquadOrder> orders;


    [SerializeField]
    private bool _AttackMode = false;
    public bool AttackMode { get { return _AttackMode; } set { _AttackMode = value; } }

    public MovementSignaler EntityTracker { get; protected set; }
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    //protected HashSet<int> TargetsToHunt;

    // Use this for initialization
    protected override void InitAwake()
    {
        base.InitAwake();
        C_GroupManager = GetComponent<GroupManager>();
        C_Movement = GetComponent<A_GridMover>();       
        orders = new Queue<A_SquadOrder>();

        //TargetsToHunt = new HashSet<int>();

        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<SquadAggresionRange>();
        C_AggresionSphere = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<SquadAggresionRange>();
    }

    protected override void InitStart()
    {
        base.InitStart();
        C_GridMovement.BaseSpeed = 9f;
        C_AggresionSphere.TrackingRange = 10f;
        //LoadStats();
    }

    protected override void GroupLogic()
    {
        if(EntityTracker != null)
        {
            if(EntityTracker.MovementSignal)
            {
                MoveToEntity(EntityTracker.TrackedEntity);
                EntityTracker.AcknowledgeSignal();
            }
        }
    }

    #region MovementControls
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
    #endregion

    #region AttackOrders
    public void StartHuntingTarget(TaggableEntity entityIn)
    {
        if (EntityTracker != null)
        {
            EntityTracker.KillSignaler();
            EntityTracker = null;
        }
        EntityTracker = new MovementSignaler(entityIn, 1f);
        MoveToEntity(entityIn);
    }

    public void AddHuntTargetOrder(I_Entity entityIn)
    {
        StartHuntingTarget((TaggableEntity)entityIn);
    }
    #endregion

    #region MovementOrders
    protected bool movingToEntity = false;
    public void MoveToEntity(TaggableEntity entityIn)
    {
        movingToEntity = true;
        C_GridMovement.SetPathToPoint(entityIn.Pos);
    }

    public void MoveToEntity(I_Entity entityIn)
    {
        movingToEntity = true;
        C_GridMovement.SetPathToPoint(entityIn.Pos);
    }

    public void MoveToPosition(Vector3 posIn)
    {
        C_GridMovement.SetPathToPoint(posIn);
    }

    public void MoveToPosition(Node posIn)
    {
        C_GridMovement.SetPathToNode(posIn);
    }
    #endregion

    public void AddOrder(A_SquadOrder orderIn)
    {
        if (CurrentOrder == null)
        {
            if(orderIn.OrderValidation())
                CurrentOrder = orderIn;
            return;
        }
        orders.Enqueue(orderIn);
    }

    public void OrderExited(bool previousOrderCompleted)
    {
        if (EntityTracker != null)
        {
            EntityTracker.KillSignaler();
            EntityTracker = null;
        }
        while (orders.Count > 0)
        {
            A_SquadOrder nextOrder = orders.Dequeue();
            var a = nextOrder.OrderValidation();
            if (nextOrder.OrderValidation())
            {
                CurrentOrder = nextOrder;
                return;
            }
        }
        CurrentOrder = null;
    }

    public void CancelOrder()
    {
        CurrentOrder.CancelOrder();
    }

    //public void CancelOrder()
    //{
    //    if (EntityTracker != null)
    //    {
    //        EntityTracker.KillSignaler();
    //        EntityTracker = null;
    //    }
    //}

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
    public A_GridMover C_GridMovement { get; protected set; }

    public GroupManager C_GroupManager { get; protected set; }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }

    public I_Sight C_AggresionSphere { get; protected set; }    
    #endregion
}

