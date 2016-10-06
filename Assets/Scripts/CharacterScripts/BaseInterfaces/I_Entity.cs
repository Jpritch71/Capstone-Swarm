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

    float groundPosY
    {
        get;
    }

    /*
     * Use this to set or get the character's position
     * Get - Gets the current position
     * Set - Sets the position, offseting the value so that the collider is resting on the ground.
     * */
    Vector3 Pos
    {
        get;
    }

    GameObject _AttachedGameObject
    {
        get;
    }
}
