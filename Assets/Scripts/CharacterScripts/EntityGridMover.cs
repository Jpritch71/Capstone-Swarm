using UnityEngine;
using System.Collections;
using System;

public class EntityGridMover : E_GridedMovement
{
    public override void TargetNodeReached()
    {
        Moving = false;
    }

    protected override void InitAwake()
    {
        print("we awaked");
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
}
