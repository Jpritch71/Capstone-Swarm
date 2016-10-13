using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))] //total volume presence of the group
public class GroupManager : Initializer
{
    private Vector3 lastPos, deltaMovement;
    private int gridedWeight;

    private GameObject bar;
    protected Transform squadObject;
    protected bool continueRunning = true;

    protected override void InitAwake()
    {
        volumeCollider = GetComponent<SphereCollider>();

        GroupGridComponent = GetComponent<A_GridMover>();
        HashSet<GroupUnitMovement> temp = new HashSet<GroupUnitMovement>();
        squadObject = transform.Find("squad");
        foreach (GroupUnitMovement g in squadObject.gameObject.GetComponentsInChildren<GroupUnitMovement>())
        {
            temp.Add(g);
            g.Squad = this;
        }
        squad = temp;

        squadObject.name = name + ": " + squadObject.name;
        squadObject.parent = GameObject.Find("EntityHierarchy").transform;
    }

    protected override void InitStart()
    {
        lastPos = transform.position;

        bar = Instantiate(new GameObject()) as GameObject;
        bar.name = "Center of Mass";
        bar.transform.position = this.transform.position;
        bar.transform.parent = this.transform;

        gridedWeight = squad.Count * 4;
        if (squad.Count <= 1)
        {
            SetVolumeRadius(1f);
            volumeCollider.radius = 1f;
        }
        else
        {
            currentVolumeRadius = volumeCollider.radius;
        }
        /////TODO change starting radius physical
        foreach (GroupUnitMovement unit in squad)
        {
            unit.SquadSendInit();
        }
        //StartCoroutine(UpdateCenterOfMass());
    }

    // Update is called once per frame
    void Update()
    {
        deltaMovement = transform.position - lastPos;

        foreach (GroupUnitMovement unit in squad)
        {
            unit.MovementOrders += deltaMovement;
        }
        lastPos = transform.position;
        UpdateCenterOfMass();
    }

    protected int unitsLost = 0;
    protected HashSet<int> IDs_OftheLost = new HashSet<int>();
    public void UnitLost(ref GroupUnitMovement unitIn)
    {
        if (unitsLost == 0)
        {
            StartCoroutine(Regroup());
        }
        if (!IDs_OftheLost.Contains(unitIn.ComponentOwner.C_Entity.Unique_ID))
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

    public Vector3 CenterOfMass { get; private set; }
    private void UpdateCenterOfMass()
    {
        float momentX;
        float momentY;
        float momentZ;

        //while (continueRunning)
        //{
            momentX = transform.position.x * gridedWeight;
            momentY = transform.position.y * gridedWeight;
            momentZ = transform.position.z * gridedWeight;

            foreach (GroupUnitMovement unit in squad)
            {
                momentX += unit.Pos.x;
                momentY += unit.Pos.y;
                momentZ += unit.Pos.z;
            }

            CenterOfMass = new Vector3(momentX / (squad.Count + gridedWeight), momentY / (squad.Count + gridedWeight), momentZ / (squad.Count + gridedWeight));
            bar.transform.position = CenterOfMass;
            //yield return new WaitForSeconds(.11f);
        //}
    }

    #region components

    public A_GridMover GroupGridComponent { get; private set; }

    public HashSet<GroupUnitMovement> squad { get; private set; }
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

    private void SetVolumeRadius(float radiusIn)
    {
        currentVolumeRadius = radiusIn;
        foreach (GroupUnitMovement g in squad)
        {
            g.DistanceThreshold = radiusIn;
        }
    }

    protected HashSet<GameObject> neighborColliders = new HashSet<GameObject>();
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
        bitwise = (1 << other.gameObject.layer) & WorldManager.entityFlag;
        //if (bitwise != 0) 
        //{
        //    blockingEntity = other;   
        //    GroupGridComponent.PauseMoving();
        //}
        if (other.gameObject.layer == 13 || bitwise != 0)
        {
            if (neighborColliders.Contains(other.gameObject))
                return;
            neighborColliders.Add(other.gameObject);
            if (volumeCollider.radius == currentVolumeRadius)
                SetVolumeRadius(currentVolumeRadius / 2f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //if(other == blockingEntity)
        //{
        //    GroupGridComponent.ResumeMoving();
        //    blockingEntity = null;
        //    return;
        //}
        if (neighborColliders.Contains(other.gameObject))
        {
            neighborColliders.Remove(other.gameObject);
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
    #endregion
}
