using UnityEngine;
using System;
using System.Collections.Generic;


public class StateMachine
{
    public I_State CurrentState { get; protected set; }
    public I_State PreviousState { get; protected set; }
    public I_Controller MachineController { get; protected set; }

    protected List<I_GlobalState> globalStates;

    public StateMachine(I_Controller entityIn, I_State startState)
    {
        MachineController = entityIn;
        CurrentState = startState;
        CurrentState.OnStart();

        globalStates = null;
    }

    public StateMachine(I_Controller controllerIn, I_State startState, List<I_State> globals)
    {
        MachineController = controllerIn;
        CurrentState = startState;
        CurrentState.OnStart();

        globalStates = new List<I_GlobalState>();
    }

    public void ExecuteUpdate()
    {
        if (globalStates != null)
        {
            foreach (I_GlobalState gState in globalStates)
            {
                if (CurrentState == gState)
                    continue;
                if (gState.IsStateValid())
                {
                    SetCurrentState(gState);
                    return; 
                }
            }
        }
        CurrentState.Execute();
    }

    public void SetCurrentState(I_State stateIn)
    {
        CurrentState.OnStopped(); //tell the previous state it is stopped
        PreviousState = CurrentState;

        CurrentState = stateIn; //set the new state
        CurrentState.OnStart(); //tell the new state it has started.
    }

    public void ReturnToPreviousState()
    {
        CurrentState.OnStopped(); //tell the previous state it is stopped

        CurrentState = PreviousState; //set the new state
        CurrentState.OnStart(); //tell the new state it has started.
    }
}

