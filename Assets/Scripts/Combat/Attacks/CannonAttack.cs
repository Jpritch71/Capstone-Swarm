using UnityEngine;
using Combat;
using System;

public class CannonAttack : A_Attack
{
    public RangedWeapon AttackingWeapon_Ranged { get { return (RangedWeapon)AttackingWeapon; } }

    public CannonAttack(I_Entity attackingEntityIn, RangedWeapon weaponIn, float durationIn, string attackNameIn) : base(attackingEntityIn, AttackMethod.Melee, weaponIn, durationIn, attackNameIn)
    {
        MethodOfAttack = AttackMethod.Firearm;
    }

    public override Vector3 PointOfOrigin
    {
        get
        {
            return AttackingWeapon.AttackPoint;
        }
    }

    protected override void ApplyAttack()
    {
        var target = attackingEntity.Owner_C_Controller.Target;
        if (target != null)
        {
            AttackingWeapon_Ranged.Launch(AttackingWeapon.AttackPoint + (target.Pos - Vector3.up - AttackingWeapon.AttackPoint),
                                            attackingEntity.Owner_C_Controller.EntityLayer);
        }
        else
        {
            AttackingWeapon_Ranged.Launch(AttackingWeapon.AttackPoint +
                                            attackingEntity.Owner_C_Controller.C_Movement.transform.forward,
                                                attackingEntity.Owner_C_Controller.EntityLayer);
        }

    }
}
