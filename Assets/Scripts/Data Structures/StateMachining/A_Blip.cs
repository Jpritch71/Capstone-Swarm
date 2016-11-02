using UnityEngine;
using System.Collections;

/// <summary>
/// A Blip a pseudo state that modifies some of the properties of a caller state.
/// </summary>
public abstract class A_Blip
{
    private StateMachine callingMachine;

    public A_Blip(StateMachine callingMachineIn)
    {
        callingMachine = callingMachineIn;
    }

    public void ExecuteBlip()
    {
        BlipAction();
        callingMachine.ReturnToPreviousState();
    }

    protected abstract void BlipAction();
}
