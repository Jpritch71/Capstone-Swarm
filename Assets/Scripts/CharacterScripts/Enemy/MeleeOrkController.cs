using UnityEngine;
using System.Collections;
using System;

public class MeleeOrkController : OrkUnitController
{
    protected override void InitAwake()
    {
        C_Movement = GetComponent<GroupUnitMovement>();
        AnimController = new GrouchoAnimController(transform.Find("Groucho").GetComponent<Animator>());
        C_StateMachine = new StateMachine(this, new S_Ork_Idle(this));
    }

    // Update is called once per frame
    void Update()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine
    }

    public override void LoadStats()
    {
        throw new NotImplementedException();
    }
}
