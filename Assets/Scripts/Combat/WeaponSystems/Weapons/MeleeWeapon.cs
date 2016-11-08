using UnityEngine;
using Combat;
using System;

public class MeleeWeapon : Weapon
{
    public MeleeWeapon(Transform weaponObjectIn, WeaponType weaponTypeIn) : base(weaponObjectIn, weaponTypeIn)
    {

    }

    public MeleeWeapon(WeaponContainer containerIn, MonoBehaviour attackTimerKillerIn,
                        Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn,
                                                    string nameIn, int soulsIn, float powerIn)
        : base(containerIn, attackTimerKillerIn, weaponObjectIn, weaponTypeIn, rangeIn, nameIn, soulsIn, powerIn)
    {

    }

    public override Collider[] AffectedColliders()
    {
        return Physics.OverlapSphere(AttackPoint, WeaponRange, WorldManager.entityFlag, QueryTriggerInteraction.Collide);
    }
}
