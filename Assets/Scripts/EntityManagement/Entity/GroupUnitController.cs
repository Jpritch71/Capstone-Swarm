using UnityEngine;
using System.Collections;
using System;

public abstract class GroupUnitController : Initializer, I_Controller
{
    public abstract int EntityLayer { get; }
    protected override void InitAwake()
    {
        C_Movement = GetComponent<GroupUnitMovement>();
        C_Movement.C_Controller = this;
        C_SquadController = C_AttachedGameObject.transform.parent.parent.GetComponent<SquadController>();
        LoadStats();
    }

    protected override void InitStart()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine
    }

    //protected abstract void MoveToTarget();

    #region Components
    public GroupUnitMovement C_Movement
    {
        get; protected set;
    }
    public I_Entity C_Entity { get; protected set; }
    public StateMachine C_StateMachine { get; protected set; }
    public SquadController C_SquadController { get; protected set; }
    #endregion

    public abstract void LoadStats();
}
