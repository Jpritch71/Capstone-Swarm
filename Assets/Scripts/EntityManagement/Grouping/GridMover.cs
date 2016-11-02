using UnityEngine;
using System.Collections;
using System;

public class GridMover : A_GridMover
{
    protected override void InitAwake()
    {
        base.InitAwake();
        thruster = GetComponent<Rigidbody>();
        BaseSpeed = 15f;
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
