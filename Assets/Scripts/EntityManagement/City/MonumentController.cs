using UnityEngine;
using System.Collections;
using System;

public class MonumentController : Initializer, I_Controller
{
    public I_Entity C_Entity
    {
        get; protected set;
    }

    public GroupUnitMovement C_Movement
    {
        get
        {
            return null;
        }
    }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }

    public int EntityLayer
    {
        get { return (int)Flags.Player; }
    }

    public I_Entity Target
    {
        get
        {
            return null;
        }
    }

    public void LoadStats()
    {
        C_Entity = new ObjectiveStructure(this, 1000f, 1);
    }

    protected override void InitAwake()
    {

    }

    protected override void InitStart()
    {
        LoadStats();
    }
}
