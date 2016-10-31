using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))] //total volume presence of the group
public abstract class GroupManager : Initializer
{
    public virtual int GroupLayer { get { return (1 << 12); } }
    protected Vector3 lastPos, deltaMovement;
    protected int gridedWeight;

    protected Transform squadObject;
    protected bool continueRunning = true;

    public GroupUnitMovement LeaderUnit { get; protected set; }

    protected override void InitAwake()
    {
        C_Squad_HashSet = new HashSet<GroupUnitMovement>();
        C_SquadControllers_HashSet = new HashSet<GroupUnitController>();
        C_Squad_List = new List<GroupUnitMovement>();
        transform = gameObject.transform;
        volumeCollider = GetComponent<SphereCollider>();
        GroupGridComponent = GetComponent<A_GridMover>();
        squadObject = transform.Find("squad");
    }

    protected override void InitStart()
    { 
        foreach (GroupUnitMovement g in squadObject.gameObject.GetComponentsInChildren<GroupUnitMovement>())
        {
            AddSquadMember(g);
        }
        LeaderUnit = C_Squad_List[0];

        squadObject.name = name + ": " + squadObject.name;
        squadObject.parent = GameObject.Find("EntityHierarchy").transform;

        lastPos = transform.position;

        gridedWeight = C_Squad_HashSet.Count * 4;

        currentVolumeRadius = volumeCollider.radius;
        /////TODO change starting radius physical
        foreach (GroupUnitMovement unit in C_Squad_HashSet)
        {
            unit.SquadSendInit();
        }
        //StartCoroutine(UpdateCenterOfMass());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGroup();
        GroupLogic();
    }

    protected virtual void UpdateGroup()
    {
        deltaMovement = transform.position - lastPos;

        foreach (GroupUnitMovement SelectedUnit in C_Squad_HashSet)
        {
            SelectedUnit.MovementOrders += deltaMovement;
        }
        lastPos = transform.position;
        UpdateCenterOfMass();
    }

    protected abstract void GroupLogic();

    protected int unitsLost = 0;
    protected HashSet<int> IDs_OftheLost = new HashSet<int>();
    public void UnitLost(ref GroupUnitMovement unitIn)
    {
        if (unitsLost == 0)
        {
            StartCoroutine(Regroup());
        }
        if (!IDs_OftheLost.Contains(unitIn.C_Controller.C_Entity.Unique_ID))
        {
            unitsLost++;
        } 
    }

    public void UnitRecovered()
    {
        unitsLost--;
        if (unitsLost == 0)
            StopCoroutine(Regroup());
    }

    protected IEnumerator Regroup()
    {
        GroupGridComponent.SpeedModifier = .5f;
        yield return new WaitForSeconds(.5f);
        while(unitsLost > 0)
        {
            GroupGridComponent.SpeedModifier -= .05f;
            yield return new WaitForSeconds(.5f);
        }
    }

    public Vector3 CenterOfMass { get; protected set; }
    protected void UpdateCenterOfMass()
    {
        float momentX;
        float momentY;
        float momentZ;

        //while (continueRunning)
        //{
        momentX = Pos.x * gridedWeight;
        momentY = Pos.y * gridedWeight;
        momentZ = Pos.z * gridedWeight;

        foreach (GroupUnitMovement unit in C_Squad_HashSet)
        {
            momentX += unit.Pos.x;
            momentY += unit.Pos.y;
            momentZ += unit.Pos.z;
        }
        momentX += LeaderUnit.Pos.x;
        momentY += LeaderUnit.Pos.y;
        momentZ += LeaderUnit.Pos.z;

        CenterOfMass = new Vector3(momentX / (C_Squad_HashSet.Count + 1 + gridedWeight), momentY / (C_Squad_HashSet.Count + 1 + gridedWeight), momentZ / (C_Squad_HashSet.Count + 1 + gridedWeight));
        //}
    }

    protected void AddSquadMember(GroupUnitMovement unitIn)
    {
        C_Squad_List.Add(unitIn);
        C_Squad_HashSet.Add(unitIn);
        GroupUnitController unitController = unitIn.C_Controller_Unit;
        C_SquadControllers_HashSet.Add(unitController);
        unitIn.Squad = this;
    }

    #region components
    public A_GridMover GroupGridComponent { get; protected set; }

    public List<GroupUnitMovement> C_Squad_List { get; protected set; }
    public HashSet<GroupUnitMovement> C_Squad_HashSet { get; protected set; }

    public HashSet<GroupUnitController> C_SquadControllers_HashSet { get; protected set; }
    #endregion

    /*
        The volume collider-trigger is used to detect obstacles near the group and respond accordingly.
        When near an obstacle, reduce the volume size to keep the group closer together.
    */
    #region WorldVolumeControl    
    protected SphereCollider volumeCollider;
    public SphereCollider VolumeCollider
    {
        get { return volumeCollider; }
    }
    protected float currentVolumeRadius;
    public float CurrentVolumeRadius
    {
        get { return currentVolumeRadius; }
    }

    protected virtual void SetVolumeRadius(float radiusIn)
    {
        currentVolumeRadius = radiusIn;
        if (C_Squad_HashSet == null)
            return;
        foreach (GroupUnitMovement g in C_Squad_HashSet)
        {
            g.DistanceThreshold = radiusIn;
        }
        int a = 0;
    }

    protected HashSet<Collider> neighborColliders = new HashSet<Collider>();
    protected bool obstacleNear = false;
    public bool ObstacleNear
    {
        get
        {
            return obstacleNear;
        }
        set { obstacleNear = value; }
    }

    protected Collider blockingEntity;
    private int bitwise;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(squadObject))
            return;
        bitwise = (1 << other.gameObject.layer) & (int)Flags.Obstacles;
        //if (bitwise != 0) 
        //{
        //    blockingEntity = other;   
        //    GroupGridComponent.PauseMoving();
        //}
        if (bitwise != 0)
        {
            if (neighborColliders.Contains(other))
                return;
            neighborColliders.Add(other);
            if (volumeCollider.radius == currentVolumeRadius)
                SetVolumeRadius(currentVolumeRadius / 2f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (neighborColliders.Contains(other))
        {
            neighborColliders.Remove(other);
            if (neighborColliders.Count <= 0)
            {
                SetVolumeRadius(volumeCollider.radius);
            }
        }
    }

    /*
     * Call upon another movable entity entering the trigger area.
     * Send a message to the other entity, if the other entity is closer to its goal block self
     * otherwise, block the other entity
     * */
    public void TryToBlockMovement(GroupManager otherGroup)
    {
        throw new System.NotImplementedException();
    }

    public Vector3 Pos
    {
        get
        {
            return transform.position;
        }
        protected set
        {
            transform.position = value;
        }
    }
    protected new Transform transform;
    #endregion
}
