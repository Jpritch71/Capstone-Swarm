using UnityEngine;
using System.Runtime.CompilerServices; 
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : E_GridedMovement 
{
    private bool directionalMovement = false;
    public bool DirectionalMovement { get { return directionalMovement; } }

    public override bool Moving
    {
        get
        {
            return moving || directionalMovement;
        }
    }
	public PlayerMovement()
	{
		baseSpeed = 9f; //x units per second
	}

    void Awake()
    {
        InitAwake();
    }

    // Use this for initialization
    void Start()
	{
        InitStart();
	}

    protected override void InitAwake()
    {
        unitCollider = GetComponent<CapsuleCollider>();
    }

    protected override void InitStart()
    {

    }

    void FixedUpdate()
    {
        if (directionalMovement)
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

                var dir = hit.point;
                dir = (dir - Pos).normalized * Speed * Time.deltaTime;
                dir = Pos + dir;

                if (WorldManager._WORLD.GetTile(dir).Walkable)
                {
                    hits = Physics.SphereCastAll(transform.position + (Vector3.up * unitCollider.bounds.size.y), unitCollider.radius * .1f, Vector3.down, 200f, (1 << 8));

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
                    Pos = new Vector3(dir.x, groundYPos, dir.z);
                }
            }
        }

        if (!directionalMovement)
            GridMovement();        
    }

    public void StartFreeMoving()
    {
        directionalMovement = true;
    }

    public void EndFreeMove()
    {
        directionalMovement = false;
    }

	public override void TargetNodeReached()
	{
        Debug.Log("Target Reached");
        Moving = false;
	}
}


