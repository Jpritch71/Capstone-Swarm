using UnityEngine;
//using System.Runtime.CompilerServices; 
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CapsuleCollider))] //volume presence of the pather
public abstract class A_GridMover : Initializer, I_Movement
{
    //speed in meters per second - base speed, speedModification
    protected float baseSpeed, modSpeed;
	protected float groundYPos;
    protected Vector3 pos;

    #region PathingUtilityVariables
    protected PathObject pathObj; //object for handling pathing
	protected Node targetNode, currentTargetNode; //the end goal node, the next goal in the path
	protected Vector3 targetPositionOFFSET; //position of the target offse to match the player's size.
	protected List<Node> wayPointNodes; //the literal list of nodes for the path
    protected List<Node> pausedNodes; //list of nodes to return to after a detour
    protected Node workingNode; //variable for storing temporary node 
    protected Node lastPathStart; //last location we were when we started looking for a path

    #endregion

    protected RaycastHit[] hits; //variable for storing temporary RaycastHit array
	protected RaycastHit hit;

    protected CapsuleCollider unitCollider;
    protected Vector3 targetDirection; //direction character is moving
	protected int clearanceRequired;

    protected bool findingPath = false; //flag indicates whether this character is already looking for a path or waiting in the queue
    protected bool moving = false, canMove = true;
    public bool CanMove
    {
        get
        {
            return canMove && !pausedMoving;
        }
        protected set
        {
            canMove = value;
        }
    }
    public virtual bool Moving { get { return moving; } protected set { moving = value; } }
    protected bool targetReached = false;

	public A_GridMover () 
	{
		baseSpeed = 5f; //x units per second
		modSpeed = 1f;
        TurnSpeed = 2f;

		wayPointNodes = new List<Node> ();
		clearanceRequired = 2;

        pathObj = new PathObject();
        pathObj.clearance = clearanceRequired;
        pathObj.gridEntity = this;
        findingPath = false;
        //nodeTraveledInPath = 0;       
	}

    protected override void InitStart()
    {
        thruster = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void FixedUpdate ()
	{
		GridMovement ();
	}

    #region PATHING
    public PathObject PathObj //Pathfinding Obj necessary for thread waiting
    {
        get { return pathObj; }
    }

    private IEnumerator _CancelPathTisFrame()
    {
        _CancelPath = true;
        yield return new WaitForEndOfFrame();
        _CancelPath = false;
    }

    #region SET_PATH
    public void setPath(List<Node> pointsIn)
    {
        findingPath = false;
        targetReached = false;
        wayPointNodes = pointsIn;
        targetNode = wayPointNodes[pointsIn.Count - 1];
        CurrentTargetNode = wayPointNodes[0];
        targetPositionOFFSET = OffsetPosition(CurrentTargetNode.Pos);
    }

    public void SetPathToTarget(GameObject targetIn)
    {
        StartCoroutine(StartPathToNode(WorldManager._WORLD.GetTile(targetIn.transform.position)));
    }

    public void SetPathToTarget(I_Entity targetIn)
    {
        StartCoroutine(StartPathToNode(WorldManager._WORLD.GetTile(targetIn._MovementComponent.Pos)));
    }

    public void SetPathToPoint(Vector3 pointIn)
    {
        StartCoroutine(StartPathToNode(WorldManager._WORLD.GetTile(pointIn)));
    }

    public void SetPathToNode(Node nodeIn)
    {
        StartCoroutine(StartPathToNode(nodeIn));
    }

    public void SetDetour(Vector3 pointIn)
    {
        pausedNodes = wayPointNodes;
    }
    #endregion

    protected bool _CancelPath = false;
    protected virtual IEnumerator StartPathToNode(Node nodeIn)
	{
        if (!CanMove)
        {
            print("cant move");
            yield break;
        }
        if (findingPath && pathObj.pathFound == false)
        {
            Debug.Log("can't begin new pathfinding process");
            Debug.Log("Finding path: " + findingPath + " pathFound: " + pathObj.pathFound);
            yield break;
        }

        workingNode = nodeIn;
        Node locationNode = WorldManager._WORLD.GetTile(Pos);
        if (workingNode.Equals(targetNode) && lastPathStart != locationNode)
        {
            UnityEngine.Debug.Log("Target not changed");
            workingNode = null;
            yield break;
        }
        lastPathStart = locationNode;
        findingPath = true;

        //nodeTraveledInPath = 0;
        if (workingNode.Walkable)
        {
            targetNode = workingNode;
            if (workingNode != WorldManager._WORLD.GetTile(transform.position))
            {
                int c = 0;

                pathObj.setPath(null); //Clear the path and tell the object the path is NOT found
                pathObj.targetNode = workingNode;
                A_GridMover charMove = this;
                if(!Pathfinder._instance.GetPathAStar(ref charMove))
                {
                    findingPath = false;
                    yield break;              
                }
                
                targetReached = false;
                while (!pathObj.pathFound)
                {
                    if (pathObj.pathFailed || _CancelPath)
                    {
                        print("exit loop | _" + pathObj.pathFailed + "_" + _CancelPath + "_");
                        findingPath = false;
                        pathObj.pathFailed = false;
                        _CancelPath = false;                       
                        yield break;
                    }

                    yield return new WaitForEndOfFrame();
                    if (c >= 1000)
                    {
                        Debug.Log("break at count" + c);
                        Debug.Break();
                        _CancelPath = false;
                        yield break;
                    }
                    ++c;
                }
                wayPointNodes = pathObj.path;
                print("Algorithm Time: " + pathObj.AlgorithmTime);
                findingPath = false;
                _CancelPath = false;

                //check to see if a path was returned, //then smooth it and set our target
                if (wayPointNodes != null && wayPointNodes.Count > 0)
                {
                    CurrentTargetNode = wayPointNodes[0];
                    targetPositionOFFSET = OffsetPosition(CurrentTargetNode.Pos);
                    Moving = true;
                }
                else if(wayPointNodes.Count <= 0)
                    TargetNodeReached();
            }
            workingNode = null;
        }
        findingPath = false;
	}

    /*
     * returns the current node in this character's path if the path exists
     * used to keep track of where the character is when pathfinding is slowed down
     * */
    public Node getNodeInPath()
    {
        if (wayPointNodes != null)
            return wayPointNodes[0];
        else
            return WorldManager._WORLD.GetTile(pos);
    }

    public Node CurrentTargetNode
    {
        get { return currentTargetNode; }
        set
        {
            currentTargetNode = value;
            if (value != null)
                targetDirection = new Vector3(currentTargetNode.Pos.x, Pos.y, currentTargetNode.Pos.z) - Pos;
            else
                targetDirection = transform.forward;
        }
    }

    #endregion

    #region Movement
    public void StopMoving()
    {
        wayPointNodes = null; // set the current path to null
        StartCoroutine(_CancelPathTisFrame()); //Ensures the path is cancelled this frame, then clears the flag
    }

    protected virtual void GridMovement()
    {
        //if we have a valid path
        if (wayPointNodes != null && wayPointNodes.Count > 0)
        {
            //if we know where our next target is
            if (CurrentTargetNode != null)
            {
                SetFacingDirection();
                //if we are within the appropriate distance to the target
                if (isCharacterAtPoint(targetPositionOFFSET))
                {
                    wayPointNodes.RemoveAt(0);
                    if (wayPointNodes.Count <= 0 && CurrentTargetNode == targetNode)
                        TargetNodeReached();
                    CurrentTargetNode = null;
                }
                      
                hits = Physics.SphereCastAll(Pos + (unitCollider.bounds.extents.x * transform.forward) + (Vector3.up * unitCollider.bounds.size.y), unitCollider.radius * .1f, Vector3.down, 5f, (1 << 8));
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
                    Debug.Log("Where is the ground?");
                thruster.MovePosition(OffsetPosition(new Vector3(transform.position.x, groundYPos, transform.position.z)) + (targetPositionOFFSET - Pos).normalized * (Time.deltaTime * Speed));
            }
            else
            {
                CurrentTargetNode = wayPointNodes[0];
                targetPositionOFFSET = OffsetPosition(CurrentTargetNode.Pos);
            }
        }
        pos = Pos;
    }

    protected bool pausedMoving = false;
    public void PauseMoving()
    {
        pausedMoving = true;
    }

    public void ResumeMoving()
    {
        pausedMoving = false;
    }

    public void SetFacingDirection()
    {
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    public bool isCharacterAtPoint(Vector3 posIn)
    {
        try
        {
            if ((posIn - Pos).magnitude <= Mathf.Clamp(Time.deltaTime * Speed, unitCollider.radius, Mathf.Infinity))
                return true;
            return false;
        }
        catch(MissingReferenceException e)
        {
            return false;
        }
    }

	//find the given position, relative to the character bounds
	protected Vector3 OffsetPosition(Vector3 posIn)
	{
		return new Vector3 (posIn.x,// + this.characterCollider.bounds.extents.x / 2f,
		                    posIn.y + unitCollider.bounds.extents.y,
		                    posIn.z);// + this.characterCollider.bounds.extents.z / 2f);
	}

    protected void TurnToFaceTarget()
    {
        try
        {
            //transform.LookAt(new Vector3(target.Pos.x, Pos.y, target.Pos.z));
        }
        catch (System.NullReferenceException e)
        {
            print(e.Message);
        }
    }

    public bool TargetReached
    {
        get { return targetReached; }
    }

    public abstract void TargetNodeReached();
    #endregion

	public float BaseSpeed
	{
		get { return baseSpeed; }
		set
		{
			baseSpeed = value;
		}
	}

	public float SpeedModifier
	{
		get { return modSpeed; }
		set
		{
			modSpeed = Mathf.Clamp(value, 0, 1000);
		}
	}

	public float Speed
	{
		get { return baseSpeed * modSpeed; } 	
	}

    public float TurnSpeed { get; protected set; }

    /*
	 * Used to decide how many nodes are occupied by this character when pathing.
	 * */
	public int Clearance
	{
		get { return clearanceRequired; } 	
		set
		{
			clearanceRequired = value;
		}
	}

    public CapsuleCollider UnitCollider
    {
        get { return unitCollider; }
    }

    /*
	 * Use this to set or get the character's position
	 * Get - Gets the current position
	 * Set - Sets the position, offseting the value so that the collider is resting on the ground.
	 * */
    public Vector3 Pos
    {
        get
        {
            try
            {
                return transform.position;
            }
            catch (MissingReferenceException e)
            {
                return Vector3.zero;
            }
        }
        protected set
        {
            transform.position = value + new Vector3(0, 1f, 0);
        }
    }

    public float groundPosY { get; protected set; }

    #region components
    public Rigidbody thruster { get; set; }   
    #endregion
}


