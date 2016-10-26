﻿using UnityEngine;
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
        Component_Owner = GetComponent<I_Controller>();
        characterCollider = GetComponent<CapsuleCollider>();
        //print(this.name + " " + characterCollider.bounds.extents.y);
        collisionAvoidanceCollider = GetComponent<SphereCollider>();
        TurnSpeed = 5f;
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
    protected Vector3 nextPos;
    protected virtual void MovementPhase()
    {
        closestObstacleDistance = DistanceThreshold;
        distanceFromCenter = Vector3.Distance(Pos, Squad.CenterOfMass);
        Align = Vector3.zero;
        Cohesion = Vector3.zero;
        Separation = Vector3.zero;
        Avoid = Vector3.zero;

        if(MovementOrders != Vector3.zero) //group is moving
        {
            Align = ((MovementOrders + transform.position) - Pos) * .05f;
            MovementOrders = Vector3.zero;

            if (distanceFromCenter > DistanceThreshold)
            {
                Cohesion = (Squad.CenterOfMass - transform.position) * .5f;
            }

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

        float f = SetFacingDirection();


        SpeedModifier = Mathf.Clamp((90 - f) / 90f, .5f, 1f);
        

        if (MovementOrders != Vector3.zero) //the unit needs to update its position
        {
            Moving = true;
            nextPos = Pos + transform.forward.normalized * Speed * Time.deltaTime;

            hits = Physics.SphereCastAll(nextPos + (Vector3.up * characterCollider.bounds.size.y), characterCollider.radius * .1f, Vector3.down, 5f, (1 << 8));
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

    protected virtual void MovementPhase1()
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

        slowDownFactor = Mathf.Clamp((closestObstacleDistance / DistanceThreshold) - .5f, 0, 1f);
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
        MovementOrders += Separation * .1f;

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


        hits = Physics.SphereCastAll(Pos + (Vector3.up * characterCollider.bounds.size.y), characterCollider.radius * .1f, Vector3.down, 5f, (1 << 8));
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
        else
            //Debug.Log("Where is the ground?");
            thruster.MovePosition(ColliderOffsetPosition(new Vector3(transform.position.x, GroundPosY, transform.position.z)) + (transform.forward) * (Time.deltaTime * Speed));
        Pos = new Vector3(transform.position.x, GroundPosY, transform.position.z);

        //if (IsLost)
        //{
        //    if (!CheckLost())
        //    {
        //        IsLost = false;
        //        Alert_Found();
        //    }
        //}
        //else
        //{
        //    if(CheckLost())
        //    {
        //        IsLost = true;
        //        Alert_Lost();
        //    }
        //}

        MovementOrders = Vector3.zero;
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
            if (MovementOrders == Vector3.zero)
                return 0;
            //lookRot = Quaternion.LookRotation(new Vector3(MovementOrders.x, 0, MovementOrders.z));
            lookRot = Quaternion.LookRotation(MovementOrders);
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
    void OnTriggerEnter(Collider other)
    {
        int bitwise = (1 << other.gameObject.layer) & WorldManager.unitAvoidanceFlag;
        if (bitwise == 0) //if 0, the other collider is not on a layer we are interested in (Enemy, player, obstacle)
            return;

        var unitComponent = other.transform.parent.GetComponent<GroupUnitMovement>();

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
        int bitwise = (1 << other.gameObject.layer) & WorldManager.unitAvoidanceFlag;
        if (bitwise == 0) //if 0, the other collider is not on a layer we are interested in (Enemy, player, obstacle)
            return;

        var unitComponent = other.transform.parent.GetComponent<GroupUnitMovement>();

        if (unitComponent != null && unitComponent != this)
            neighborUnits.Remove(unitComponent);
        else if (other.gameObject.layer == 13)
        {
            neighborColliders.Remove(other.gameObject);
        }
    }

    #region components
    public Rigidbody thruster { get; set; }
    public I_Controller Component_Owner { get; set; }
    #endregion
}
