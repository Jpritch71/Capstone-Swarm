using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Combat;

public interface I_Entity
{
    WeaponContainer AttackManager { get; }
    int Unique_ID { get; }

    //bool Initialized { get; }
    bool Killable { get; }
    bool Dead { get; }
    float BaseIntegrity { get; }
    float Integrity { get; }

    void IncurDamage(float damageIn);
    void DeathAction();

    I_Controller Owner_C_Controller
    {
        get;
    }

    Collider C_Collider
    {
        get;
    }

    Entity_MonoBehaviour C_MonoBehavior { get; }

    string Name { get; }
    int EntityLevel { get; }
    CombatModifierHandler DefenseModifiers { get; }

    Vector3 Pos { get; }
}


