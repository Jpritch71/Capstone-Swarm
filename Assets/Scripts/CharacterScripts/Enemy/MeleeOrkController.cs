using UnityEngine;
using System.Collections;
using System;

public class MeleeOrkController : OrkUnitController
{
    protected override void InitAwake()
    {
        MovementComponent = GetComponent<GroupUnit>();
        AnimController = new GrouchoAnimController(GameObject.Find("Groucho").GetComponent<Animator>());
        stateController = new StateMachine(MovementComponent, new S_Ork_Idle(this));
    }

    // Update is called once per frame
    void Update()
    {
        if (stateController != null)
            stateController.ExecuteUpdate(); //update State Machine
    }
}
