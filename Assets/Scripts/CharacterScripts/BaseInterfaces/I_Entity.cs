using UnityEngine;
using System.Collections;

public interface I_Entity
{
    bool Killable { get; }
    bool Dead { get; }
    float BaseIntegrity { get; }
    float Integrity { get; }

    void IncurDamage(float damageIn);
    void DeathAction();

    GameObject _AttachedGameObject
    {
        get;
    }

    I_Movement _MovementComponent
    {
        get;
    }
}
