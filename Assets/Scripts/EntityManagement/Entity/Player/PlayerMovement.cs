using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System;

public class PlayerMovement : A_GridMover 
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
		baseSpeed = 15f; //x units per second
	}

    protected override void InitAwake()
    {
        base.InitAwake();
        unitCollider = GetComponent<CapsuleCollider>();
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
                dir = (dir - Pos).normalized * Speed * unitCollider.bounds.extents.x * Time.deltaTime;

                hits = Physics.SphereCastAll(Pos + dir + (Vector3.up * unitCollider.bounds.size.y), unitCollider.radius * .1f, Vector3.down, 5f, (1 << 8));
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

                if (WorldManager._WORLD.GetTile(Pos + dir).Walkable)
                {                   
                    thruster.MovePosition(OffsetPosition(new Vector3(Pos.x, groundYPos, Pos.z)) + dir / unitCollider.bounds.extents.x);
                }
                else
                {
                    if(Physics.Raycast(Pos, dir, out hit, 5f, (int)Flags.Obstacles))
                    {
                        dir = (hit.normal - dir.normalized) * Speed * unitCollider.bounds.extents.x * Time.deltaTime;
                        thruster.MovePosition(OffsetPosition(new Vector3(Pos.x, groundYPos, Pos.z)) + dir / unitCollider.bounds.extents.x);
                    }
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


