using UnityEngine;
using System.Collections;
using System;
using Combat;

public class MeleeAttack : A_Attack
{   
    public MeleeAttack(I_Entity attackingEntityIn, MeleeWeapon weaponIn, float durationIn, string attackNameIn) : base(attackingEntityIn, AttackMethod.Melee, weaponIn, durationIn, attackNameIn)
    {
        MethodOfAttack = AttackMethod.Melee;
    }

    public override Vector3 PointOfOrigin
    {
        get
        {
            return AttackingWeapon.AttackPoint;
        }
    }

    protected Collider[] collidersHit;
    protected override void ApplyAttack()
    {         
        collidersHit = AttackingWeapon.AffectedColliders();
        if(collidersHit.Length == 0)
        {
            return;
        }
        I_Entity en = null;
        foreach(Collider c in collidersHit)
        {
            if (c == attackingEntity.C_Collider)
                continue;
            if (c.gameObject.layer == attackerLayer)
                continue;
            if(EntityManager.GetEntityByCollider(c, ref en))
            DamageControl.DamageController.Instance.AddDamageToTarget(en, AttackingWeapon.BaseAttackPower);
        }
        collidersHit = null;
    }
}
