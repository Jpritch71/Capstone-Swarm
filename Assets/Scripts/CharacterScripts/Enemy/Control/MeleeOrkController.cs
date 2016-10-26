using UnityEngine;
using System.Collections;
using System;

public class MeleeOrkController : OrkUnitController
{
    public Transform weapon;
    protected override void InitAwake()
    {
        base.InitAwake();
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
        C_Entity = new OrkEntity(this, 100f, 1);
        MeleeWeapon OrkAxe = new MeleeWeapon(weapon, Combat.WeaponType.Axe, 1f, "Staff Axe", 0);
        C_Entity.AttackManager.AddAttack(0, new MeleeAttack(C_Entity, OrkAxe, 1f));
    }
}
