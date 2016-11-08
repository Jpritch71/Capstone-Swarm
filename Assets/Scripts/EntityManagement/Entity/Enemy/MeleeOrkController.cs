using UnityEngine;
using System.Collections;
using System;

public class MeleeOrkController : OrkUnitController
{
    public override int EntityLayer { get { return 12; } }
    public Transform weapon;

    protected override void InitAwake()
    {
        base.InitAwake();
        AnimController = new GrouchoAnimController(transform.Find("Groucho").GetComponent<Animator>());
        C_StateMachine = new StateMachine(null, new S_Ork_Idle(this));
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
        MeleeWeapon OrkAxe = new MeleeWeapon(C_Entity.WeaponManager, C_Entity.C_MonoBehavior, weapon, Combat.WeaponType.Axe,
                                                2.5f, "Staff Axe", 0, 100f);
        C_Entity.WeaponManager.AddWeapon(OrkAxe.Unique_ID, OrkAxe);
        OrkAxe.AddAttack(0, new MeleeAttack(C_Entity, OrkAxe, 2f, "Axe Strike"));       
    }
}
