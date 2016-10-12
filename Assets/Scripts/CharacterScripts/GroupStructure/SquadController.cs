using UnityEngine;
using System.Collections;
using System;

public abstract class SquadController : Initializer, I_Controller
{
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    // Use this for initialization
    protected override void InitAwake()
    {
        C_Movement = GetComponent<A_GridMover>();
    }

    protected override void InitStart()
    {
        LoadStats();
    }

    public abstract void LoadStats();

    #region Components
    public I_Movement C_Movement
    {
        get
        {
            return C_GridMovement;
        }
        protected set
        {
            C_GridMovement = (A_GridMover)value;
        }
    }
    public A_GridMover C_GridMovement { get; private set; }

    public I_Entity C_Entity
    {
        get; protected set;
    }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }
    #endregion
}
