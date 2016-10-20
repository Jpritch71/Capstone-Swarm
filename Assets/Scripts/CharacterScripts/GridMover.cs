using UnityEngine;
using System.Collections;
using System;

public class GridMover : A_GridMover
{
    protected override void InitAwake()
    {
        unitCollider = GetComponent<CapsuleCollider>();
        thruster = GetComponent<Rigidbody>();
        BaseSpeed = 15f;
    }

    protected override void InitStart()
    {
        
    }

    void FixedUpdate()
    {
        GridMovement();
    }

    public override void TargetNodeReached()
    {
        Moving = false;
    }
}
