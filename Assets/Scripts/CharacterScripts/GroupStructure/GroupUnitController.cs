using UnityEngine;
using System.Collections;
using System;

public abstract class GroupUnitController : Initializer, I_Controller
{
    protected override void InitAwake()
    {
        C_Movement = GetComponent<GroupUnitMovement>();
        C_Movement.Component_Owner = this;
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

    #region Components
    public I_Movement C_Movement
    {
        get
        {
            return C_UnitMovement;
        }
        protected set
        {
            C_UnitMovement = (GroupUnitMovement)value;
        }
    }
    public GroupUnitMovement C_UnitMovement
    {
        get; protected set;
    }
    public I_Entity C_Entity { get; protected set; }
    public StateMachine C_StateMachine { get; protected set; }
    #endregion

    public abstract void LoadStats();
}
