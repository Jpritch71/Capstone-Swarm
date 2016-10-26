using UnityEngine;
using Combat;
using System;

public class MeleeWeapon : Weapon
{
    public MeleeWeapon(Transform weaponObjectIn, WeaponType weaponTypeIn) : base(weaponObjectIn, weaponTypeIn)
    {

    }

    public MeleeWeapon(Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn, string nameIn, int soulsIn) : base(weaponObjectIn, weaponTypeIn, rangeIn, nameIn, soulsIn)
    {

    }

    public override Collider[] AffectedColliders()
    {
        return Physics.OverlapSphere(AttackPoint, .5f, WorldManager.entityFlag, QueryTriggerInteraction.Collide);
    }
}
