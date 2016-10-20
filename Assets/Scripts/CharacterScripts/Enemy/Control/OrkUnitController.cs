using UnityEngine;
using System.Collections;

public abstract class OrkUnitController : GroupUnitController
{
    // Update is called once per frame
    void Update()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine
    }

    #region Components
    public GrouchoAnimController AnimController { get; protected set; }
    #endregion
}
