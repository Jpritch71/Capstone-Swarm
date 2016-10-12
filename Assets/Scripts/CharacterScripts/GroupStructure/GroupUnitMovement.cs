using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GroupUnitMovement : Initializer, I_Movement
{
    //speed in meters per second - base speed, speedModification
    protected float groundYPos;
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
        characterCollider = GetComponent<CapsuleCollider>();
        print(this.name + " " + characterCollider.bounds.extents.y);
        collisionAvoidanceCollider = GetComponent<SphereCollider>();
    }

    protected override void InitStart()
    {
        thruster = GetComponent<Rigidbody>();
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
    protected virtual void MovementPhase()
    {
        closestObstacleDistance = DistanceThreshold;
        distanceFromCenter = Vector3.Distance(Pos, Squad.CenterOfMass);
        Align = Vector3.zero;
        Cohesion = Vector3.zero;
        Separation = Vector3.zero;
        Avoid = Vector3.zero;

        if (MovementOrders != Vector3.zero && distanceFromCenter > distanceThreshold)
        {
            Align = ((MovementOrders + transform.position) - Pos) * .05f;
            MovementOrders = Vector3.zero;
            if (distanceFromCenter > DistanceThreshold)
            {
                Cohesion = (Squad.CenterOfMass - transform.position) * .5f;
            }
        }
        else if (distanceFromCenter > DistanceThreshold * 2)
        {
            Cohesion = (Squad.CenterOfMass - transform.position);
        }

        //are there any detected units within the distance separation-trigger area
        if (neighborUnits.Count > 0)
        {
            foreach (GroupUnitMovement unit in neighborUnits)
            {
                distance = Vector3.Distance(unit.Pos, transform.position);
                if (distance < DistanceThreshold)
                {
                    Separation += (unit.Pos - transform.position) * .1f;
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
                distance = Vector3.Distance(neighborObject.transform.position, transform.position);
                if (distance < distanceThreshold)
                {
                    Separation += (neighborObject.transform.position - transform.position) * 1 / distance;
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

        RaycastHit hit;
        //look for obstacles directly in front of the unit, try to avoid
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceThreshold / 2f, WorldGrid.mapFlag, QueryTriggerInteraction.Ignore))
        {
            Avoid = (transform.position + (transform.forward * distanceThreshold / 2f) - hit.point);
            Avoid = Avoid.normalized;
            Debug.DrawRay(Pos, Avoid.normalized * distanceThreshold / 2f, Color.green);

        }

        slowDownFactor = closestObstacleDistance / DistanceThreshold;
        Avoid *= slowDownFactor;
        Separation *= slowDownFactor;
        //slowDownFactor = Mathf.Clamp(closestObstacleDistance / DistanceThreshold, .5f, SpeedModifier);

        if (Physics.Raycast(transform.position, (Squad.CenterOfMass - transform.position), Vector3.Distance(Squad.CenterOfMass, transform.position), WorldGrid.mapFlag, QueryTriggerInteraction.Ignore))
            Cohesion *= .25f;

        if (Moving)
        {
            MovementOrders += Avoid;
        }
        MovementOrders += Align;
        MovementOrders += Cohesion * (Mathf.Clamp(distanceFromCenter + DistanceThreshold, 0, 20f) / 20f) * .1f;
        MovementOrders += Separation * .3f;
               
        SetFacingDirection();
        if (MovementOrders != Vector3.zero)
        {
            //if (distance < DistanceThreshold)
            //    SpeedModifier = .9f;
            //else
            //    SpeedModifier = 1.01f;
  
            //SpeedModifier = Mathf.Clamp(closestObstacleDistance / DistanceThreshold, .5f, SpeedModifier);
            transform.position += transform.forward.normalized * Speed * Time.deltaTime;
            Moving = true;
        }
        else
        {
            Moving = false;
        }


        hits = Physics.SphereCastAll(Pos + (characterCollider.bounds.extents.x * transform.forward) + (Vector3.up * characterCollider.bounds.size.y), characterCollider.radius * .1f, Vector3.down, 5f, (1 << 8));
        if (hits.Length > 0)
        {
            hit = hits[0];
            for (int x = 1; x < hits.Length; x++)
            {
                if (hit.point.y < hits[x].point.y)
                    hit = hits[x];
            }
            groundYPos = hit.point.y;
        }
        else
            //Debug.Log("Where is the ground?");
        thruster.MovePosition(ColliderOffsetPosition(new Vector3(transform.position.x, groundYPos, transform.position.z)) + (transform.forward) * (Time.deltaTime * Speed));
        Pos = new Vector3(transform.position.x, groundYPos, transform.position.z);
        MovementOrders = Vector3.zero;
    }

    public bool CheckLost()
    {
        return distanceFromCenter > Squad.CurrentVolumeRadius * 1.1f;
    }

    protected void Alert_Lost()
    {

    }

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
    public void SetFacingDirection()
    {
        try
        {
            if (MovementOrders == Vector3.zero)
                return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(MovementOrders.x, 0, MovementOrders.z)), Time.deltaTime * 5f);
        }
        catch (System.NullReferenceException e)
        {

        }
    }

    public bool isCharacterAtPoint(Vector3 posIn)
    {
        try
        {
            if ((posIn - Pos).magnitude <= Mathf.Clamp(Time.deltaTime * Speed, characterCollider.bounds.extents.x, Mathf.Infinity))
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

    public float Speed
    {
        get { return BaseSpeed * SpeedModifier; }
    }

    public float TurnSpeed { get; protected set; }

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

    public float groundPosY
    {
        get
        {
            throw new NotImplementedException();
        }
    }
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
    void OnTriggerEnter(Collider other)
    {
        var unitComponent = other.GetComponent<GroupUnitMovement>();

        if (unitComponent != null)
        {
            if (unitComponent != this && Squad.squad.Contains(unitComponent))
            {
                neighborUnits.Add(unitComponent);
            }
        }
        else if (other.gameObject.layer == 13)
        {
            neighborColliders.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var unitComponent = other.GetComponent<GroupUnitMovement>();

        if (unitComponent != null && unitComponent != this)
            neighborUnits.Remove(unitComponent);
        else if (other.gameObject.layer == 13)
        {
            neighborColliders.Remove(other.gameObject);
        }
    }

    #region components
    public Rigidbody thruster { get; set; }
    #endregion
}
