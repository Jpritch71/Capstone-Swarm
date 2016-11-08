using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

public class GroupUnitMovement : Initializer, I_Movement
{
    //speed in meters per second - base speed, speedModification
    public float GroundPosY { get; protected set; }
    protected Vector3 lastPos;
    protected float distanceFromCenter;
    private float distanceThreshold;
    public float DistanceThreshold
    {
        get
        {
            return distanceThreshold;
        }
        set
        {
            distanceThreshold = value / 2f;
            //collisionAvoidanceCollider.radius = distanceThreshold;
        }
    }

    //variable for storing temporary node
    protected RaycastHit[] hits; //variable for storing temporary RaycastHit array
    protected RaycastHit hit;

    protected CapsuleCollider characterCollider;
    protected Vector3 movingDirection; //direction character is moving

    protected bool findingPath = false; //flag indicates whether this character is already looking for a path or waiting in the queue
    protected bool moving = false;

    protected const float notMovingThresh = .15f;
    protected float notMovingTime;
    public bool Moving
    {
        get
        {
            return moving;
        }
        protected set
        {
            if (value == false)
            {
                notMovingTime += Time.deltaTime;
                if (notMovingTime > notMovingThresh)
                {
                    moving = false;
                    thruster.velocity = Vector3.zero;
                }
                else
                    moving = true;
            }
            else
            {
                notMovingTime = 0f;
                moving = true;
                return;
            }                        
        }
    }
    protected bool targetReached = false;

    protected bool canMove = true;
    public Vector3 Align { get; private set; }
    public Vector3 Cohesion { get; private set; }
    public Vector3 Separation { get; private set; }
    public Vector3 Avoid { get; private set; }
    protected float avoidFactor;

    protected override void InitAwake()
    {      
        transform = GetComponent<Transform>();
        C_Controller = GetComponent<I_Controller>();
        characterCollider = GetComponent<CapsuleCollider>();
        //print(this.name + " " + characterCollider.bounds.extents.y);
        collisionAvoidanceCollider = GetComponent<SphereCollider>();
        TurnSpeed = 5f;
        DistanceToTarget = Mathf.Infinity;
    }

    protected override void InitStart()
    {
        thruster = GetComponent<Rigidbody>();
        StartGroupMovement();
    }

    public void SquadSendInit()
    {
        DistanceThreshold = Squad.CurrentVolumeRadius;
        avoidFactor = 1f;
    }

    void FixedUpdate()
    {
        if (CanMove)
            MovementPhase();
        lastPos = transform.position;
    }

    protected float distance, closestObstacleDistance, slowDownFactor;
    protected Vector3 nextPos;
    protected Action movementAction;
    protected void MovementPhase()
    {
        if (C_Controller_Unit.C_Entity.Dead)
            return;
        closestObstacleDistance = DistanceThreshold;
        distanceFromCenter = Vector2.Distance(new Vector2(Pos.x, Pos.z), new Vector2(Squad.CenterOfMass.x, Squad.CenterOfMass.z));
        Align = Vector3.zero;
        Cohesion = Vector3.zero;
        Separation = Vector3.zero;
        Avoid = Vector3.zero;

        movementAction.Invoke();
        
        if (MovementOrders != Vector3.zero) //the unit needs to update its position
        {
            float f = SetFacingDirection();
            SpeedModifier = Mathf.Clamp((105 - f) / 90f, .15f, 1f);

            Moving = true;
            nextPos = Pos + transform.forward.normalized * Speed * Time.deltaTime;

            hits = Physics.SphereCastAll(nextPos + (Vector3.up * 5f), characterCollider.radius * .1f, Vector3.down, 10f, (1 << 8));
            if (hits.Length > 0)
            {
                hit = hits[0];
                for (int x = 1; x < hits.Length; x++)
                {
                    if (hit.point.y < hits[x].point.y)
                        hit = hits[x];
                }
                GroundPosY = hit.point.y;
            }
            thruster.MovePosition(ColliderOffsetPosition(new Vector3(nextPos.x, GroundPosY, nextPos.z)));
        }
        else
            Moving = false;

        MovementOrders = Vector3.zero;
    }

    protected void GroupMovement()
    {
        Debug.DrawLine(Pos, Squad.CenterOfMass);

        if(MovementOrders != Vector3.zero) //group is moving
        {
            Align = ((MovementOrders + Pos) - Pos) * .05f;
            MovementOrders = Vector3.zero;

            if (distanceFromCenter > DistanceThreshold)
            {
                Cohesion = (Squad.CenterOfMass - Pos) * .5f;
            }

            if (neighborUnits.Count > 0)
            {
                foreach (GroupUnitMovement unit in neighborUnits)
                {
                    distance = Vector3.Distance(unit.Pos, Pos);
                    if (distance < DistanceThreshold)
                    {
                        Separation += (unit.Pos - Pos) * .1f;
                        if (distance < closestObstacleDistance)
                            closestObstacleDistance = distance;
                    }
                }
            }
            //are there any detected obstacles within the distance separation-trigger area
            if (neighborColliders.Count > 0)
            {
                foreach (GameObject neighborObject in neighborColliders)
                {
                    distance = Vector3.Distance(neighborObject.transform.position, Pos);
                    if (distance < distanceThreshold)
                    {
                        Separation += (neighborObject.transform.position - Pos) * 1 / distance;
                        if (distance < closestObstacleDistance)
                            closestObstacleDistance = distance;
                    }
                }
            }
            if (neighborUnits.Count > 0 || neighborColliders.Count > 0)
            {
                Separation /= (neighborUnits.Count + neighborColliders.Count);
                Separation *= -1f;
                Separation = Separation.normalized;
            }

            //look for obstacles directly in front of the unit, try to avoid
            if (Physics.Raycast(transform.position, transform.forward, out hit, distanceThreshold / 2f, WorldGrid.mapFlag, QueryTriggerInteraction.Ignore))
            {
                Avoid = (transform.position + (transform.forward * distanceThreshold / 2f) - hit.point);
                Avoid = Avoid.normalized;
                Debug.DrawRay(Pos, Avoid.normalized * distanceThreshold / 2f, Color.green);
            }

            MovementOrders = Align;
            MovementOrders += Cohesion;
            MovementOrders += Separation;
            MovementOrders += Avoid;
        }
        else //group is not moving
        {            
            if (distanceFromCenter > DistanceThreshold)
            {
                Cohesion = (Squad.CenterOfMass - transform.position);
            }

            if (neighborUnits.Count > 0)
            {
                foreach (GroupUnitMovement unit in neighborUnits)
                {
                    distance = Vector3.Distance(unit.Pos, transform.position);
                    if (distance < DistanceThreshold / 2f)
                    {
                        Separation += (unit.Pos - transform.position) * .1f;
                        if (distance < closestObstacleDistance)
                            closestObstacleDistance = distance;
                    }
                }
            }
            if (neighborUnits.Count > 0)
            {
                Separation /= (neighborUnits.Count);
                Separation *= -1f;
                Separation = Separation.normalized;
            }

            MovementOrders = Cohesion;
            MovementOrders += Separation;
        }
    }

    protected void MoveToTarget()
    {
        var target = C_Controller_Unit.C_SquadController.C_AggresionSphere.Target;
        if (target == null)
            return;
        if (Vector3.Distance(Pos, target.Pos) > C_Controller.C_Entity.WeaponManager.ActiveWeapon.WeaponRange) // && !(C_Controller_Unit.C_Entity.AttackManager.ActiveWeapon.ActiveAttack.AttackCompleted)) //the unit needs to update its position
        {
            MovementOrders = Vector3.zero;
            Align = ((target.Pos) - Pos) * .05f;

            int count = neighborUnits.Count;
            if (neighborUnits.Count > 0)
            {
                foreach (GroupUnitMovement unit in neighborUnits)
                {
                    if (unit == target.Owner_C_Controller.C_Movement)
                    {
                        count--;
                        continue;
                    }
                    distance = Vector3.Distance(unit.Pos, Pos);
                    if (distance < DistanceThreshold / 1.7f)
                    {
                        Separation += (unit.Pos - Pos) * .5f;
                        if (distance < closestObstacleDistance)
                            closestObstacleDistance = distance;
                    }
                }
            }
            if (count > 0)
            {
                Separation /= count;
                Separation *= -1f;
                Separation = Separation.normalized;
            }
            MovementOrders = Align;
            MovementOrders += Separation * 1.1f;
            MovementOrders += Avoid;
        }
        else
        {
            MovementOrders = ((target.Pos) - Pos);
            SetFacingDirection();
            MovementOrders = Vector3.zero;
        }
        DistanceToTarget = Vector3.Distance(Pos, target.Pos);
    }

    public void StartGroupMovement()
    {
        movementAction = GroupMovement;
        AttackMode = false;
    }

    public bool AttackMode { get; protected set; }
    public float DistanceToTarget { get; protected set; }
    public void StartMovingToTarget()
    {
        movementAction = MoveToTarget;
        AttackMode = true;
    }

    #region LostAndFound
    public bool IsLost
    {
        get; protected set;
    }

    public bool CheckLost()
    {
        return distanceFromCenter > Squad.CurrentVolumeRadius * 1.1f;
    }

    public bool CheckFound()
    {
        return distanceFromCenter > distanceThreshold * 1.35f;
    }

    protected void Alert_Lost()
    {
        var en = this;
        Squad.UnitLost(ref en);
    }

    protected void Alert_Found()
    {
        var en = this;
        Squad.UnitRecovered();
    }
    #endregion

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;
    }


    public void PauseMoving()
    {
        StopMoving();
    }

    #region MovementUtils
    public Vector3 MovementOrders { get; set; }
    private Quaternion lookRot;
    public float SetFacingDirection()
    {
        try
        {
            Vector3 order = new Vector3(MovementOrders.x, 0, MovementOrders.z);
            if (order == Vector3.zero)
                return 0;
            lookRot = Quaternion.LookRotation(order);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * TurnSpeed);
            return Mathf.Abs(lookRot.eulerAngles.y - transform.rotation.eulerAngles.y);
        }
        catch (System.NullReferenceException e)
        {

        }
        return 0;
    }

    public bool isCharacterAtPoint(Vector3 posIn)
    {
        try
        {
            float averagedY = posIn.y + Pos.y;
            averagedY /= 2f;
            if ((new Vector3(posIn.x, averagedY, posIn.z) - new Vector3(Pos.x, averagedY, Pos.z)).magnitude <= Mathf.Clamp(Time.deltaTime * Speed, characterCollider.bounds.extents.x, Mathf.Infinity))
                return true;
            return false;
        }
        catch (MissingReferenceException e)
        {
            return false;
        }
    }

    //find the given position, relative to the character bounds
    protected Vector3 ColliderOffsetPosition(Vector3 posIn)
    {
        return new Vector3(posIn.x,// + this.characterCollider.bounds.extents.x / 2f,
                            posIn.y + characterCollider.bounds.extents.y,
                            posIn.z);// + this.characterCollider.bounds.extents.z / 2f);
    }

    /// <summary>
    /// BaseSpeed is determined on a group level by the Group's Grid Movement Speed
    /// </summary>
    public float BaseSpeed
    {
        get { return Squad.GroupGridComponent.BaseSpeed; }
    }

    protected float speedModifier = 1f;
    public float SpeedModifier
    {
        get { return speedModifier; }
        set
        {
            speedModifier = Mathf.Clamp(value, 0, 1000);
        }
    }

    /// <summary>
    ///  Real Speed is a product of the BaseSpeed and a SpeedModifier specific to this unit.
    /// </summary>
    public float Speed
    {
        get { return BaseSpeed * SpeedModifier; }
    }

    protected float BaseTurnSpeed { get; set; }
    public float TurnSpeed
    {
        get
        {
            return Mathf.Clamp(BaseTurnSpeed * (1 / speedModifier), BaseTurnSpeed / 2f, BaseTurnSpeed * 2f);
        }
        set
        {
            BaseTurnSpeed = value;
        }
    }

    /*
	 * Used to decide how many nodes are occupied by this character when pathing.
	 * */
    public float Clearance
    {
        get { return Squad.VolumeCollider.radius; }
    }
    #endregion

    #region position
    /*
    * Use this to set or get the character's position
    * Get - Gets the current position
    * Set - Sets the position, offseting the value so that the collider is resting on the ground.
    * */

    public Vector3 Pos
    {
        get
        {
            return transform.position;
        }
        protected set
        {
            transform.position = value + new Vector3(0, characterCollider.bounds.extents.y, 0);
        }
    }
    protected new Transform transform;
    #endregion

    #region components
    public GroupManager Squad { get; set; }

    public bool CanMove
    {
        get
        {
            return canMove;
        }
        protected set
        {
            canMove = value;
        }
    }
    #endregion

    protected SphereCollider collisionAvoidanceCollider;
    protected List<GroupUnitMovement> neighborUnits = new List<GroupUnitMovement>();
    protected List<GameObject> neighborColliders = new List<GameObject>();
    protected Dictionary<GroupUnitMovement, NeighbourUnitTag> pendingUnitTags = new Dictionary<GroupUnitMovement, NeighbourUnitTag>();
    private const int AVOID_FLAG = ((int)Flags.Entities | (int)Flags.Obstacles);
    void OnTriggerEnter(Collider other)
    {
        int bitwise = (1 << other.gameObject.layer) & AVOID_FLAG;
        if (bitwise == 0) //if 0, the other collider is not on a layer we are interested in (Enemy, player, obstacle)
            return;

        if(other.gameObject.layer == 13)
        {
            neighborColliders.Add(other.gameObject);
            return;
        }
        var unitComponent = other.transform.parent.GetComponent<GroupUnitMovement>();

        if (unitComponent != null && unitComponent != this && !neighborUnits.Contains(unitComponent))// && Squad.C_Squad_HashSet.Contains(unitComponent))
        {
            AddNeighbourUnit(unitComponent);
        }
    }

    void OnTriggerExit(Collider other)
    {
        int bitwise = (1 << other.gameObject.layer) & AVOID_FLAG;
        if (bitwise == 0) //if 0, the other collider is not on a layer we are interested in (Enemy, player, obstacle)
            return;

        var a = other.transform.parent;
        if (a == null)
            return;
        var unitComponent = other.transform.parent.GetComponent<GroupUnitMovement>();

        if (unitComponent != null && unitComponent != this)
            RemoveNeighbourUnit_ClearTag(unitComponent);
        else if (other.gameObject.layer == 13)
        {
            neighborColliders.Remove(other.gameObject);
        }
    }

    public void AddNeighbourUnit(GroupUnitMovement unitIn)
    {
        neighborUnits.Add(unitIn);
        NeighbourUnitTag tag = new NeighbourUnitTag(this, unitIn);
        ((TaggableEntity)unitIn.C_Controller_Unit.C_Entity).TagEntity(tag);
        pendingUnitTags.Add(unitIn, tag);
    }

    public void RemoveNeighbourUnit(GroupUnitMovement unitIn)
    {
        neighborUnits.Remove(unitIn);    
        pendingUnitTags.Remove(unitIn);
    }

    protected void RemoveNeighbourUnit_ClearTag(GroupUnitMovement unitIn)
    {
        NeighbourUnitTag tag = null;
        if (pendingUnitTags.TryGetValue(unitIn, out tag))
        {
            ((TaggableEntity)unitIn.C_Controller_Unit.C_Entity).RemoveTag(tag);

        }
        RemoveNeighbourUnit(unitIn);
    }

    #region components
    public Rigidbody thruster { get; set; }
    public I_Controller C_Controller
    {
        get { return C_Controller_Unit; }
        set { C_Controller_Unit = (GroupUnitController)value; }
    }
    public GroupUnitController C_Controller_Unit { get; protected set; }
    #endregion
}

public class NeighbourUnitTag : I_EntityTag
{
    private GroupUnitMovement movingUnit;
    private GroupUnitMovement taggedEntity_Movement;
    public NeighbourUnitTag(GroupUnitMovement movingUnitIn, GroupUnitMovement entityIn)
    {
        movingUnit = movingUnitIn;
        taggedEntity_Movement = entityIn;
    }

    public void TagAction()
    {
        movingUnit.RemoveNeighbourUnit(taggedEntity_Movement);
    }
}
