using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Combat;

public abstract class A_Attack
{
    public abstract Vector3 PointOfOrigin { get; }

    public AttackMethod MethodOfAttack { get; protected set; }
    public I_Weapon AttackingWeapon { get; protected set; }
    public WeaponType TypeOfWeapon { get { return AttackingWeapon.TypeOfWeapon; } }
    public CombatModifierHandler AttackModifiers { get; protected set; }

    public bool CanAutoAttack { get; protected set; }

    public bool AttackInProgress { get; protected set; }
    public bool AttackCompleted { get; set; }
    public bool CanAttack { get { return !AttackInProgress && AttackCompleted; } }
    public float AttackDuration { get; protected set; }
    protected Timer attackTimer;

    protected I_Entity attackingEntity;
    protected int attackerLayer;
    

    public A_Attack()
    {       
        attackingEntity = null;
        MethodOfAttack = AttackMethod.Ethereal;
        AttackModifiers = new CombatModifierHandler();
        AttackModifiers.AddModifier(AttackAttribute.God, 1000f);
        var _GOD = GameObject.Find("GOD").transform;
        AttackingWeapon = new MeleeWeapon(_GOD ? _GOD : new GameObject("GOD").transform, WeaponType.NoPhysicalWeapon);
        attackerLayer = attackingEntity.Owner_C_Controller.C_AttachedGameObject.layer;
        CanAutoAttack = false;
        AttackDuration = 1f;
    }
    
    public A_Attack(I_Entity attackingEntityIn, AttackMethod methodIn, I_Weapon weaponIn, float durationIn)
    {
        attackingEntity = attackingEntityIn;
        MethodOfAttack = methodIn;
        AttackModifiers = new CombatModifierHandler();
        AttackingWeapon = weaponIn;
        attackerLayer = attackingEntity.Owner_C_Controller.C_AttachedGameObject.layer;
        AttackDuration = durationIn;
    }

    public void DoAttack()
    {
        OnAttackStart();
        Timer.Register(AttackDuration, () => { ApplyAttack(); OnAttackFinish(); }, null, false, false, null);
    } 

    protected abstract void ApplyAttack();

    protected virtual void OnAttackStart()
    {
        AttackCompleted = false;
        AttackInProgress = true;
    }

    protected virtual void OnAttackFinish()
    {
        AttackInProgress = false;
        AttackCompleted = true;
    }

    public void CancelAttack()
    {
        if(attackTimer != null)
            attackTimer.Cancel();
        AttackInProgress = false;
        AttackCompleted = true;
    }
}

