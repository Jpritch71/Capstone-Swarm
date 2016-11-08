using UnityEngine;
using Combat;
using System.Collections.Generic;

public abstract class Weapon
{
    public int Unique_ID { get; protected set; }
    public Dictionary<int, A_Attack> Weapon_Attacks { get; protected set; }
    public A_Attack ActiveAttack { get; protected set; }

    public Transform WeaponGameObject { get; protected set; }
    protected Transform weaponAttackPoint { get; set; }
    public Vector3 AttackPoint
    {
        get
        {
            return weaponAttackPoint.position;
        }
    }
    public string Name { get; protected set; }
    public int SoulsTaken { get; protected set; }
    public WeaponType TypeOfWeapon { get; protected set; }

    private bool _attacking = false;
    public bool Attacking
    {
        get
        {
            return _attacking;
        }
        set
        {
            _attacking = value;
            if(_attacking)
            {
                OwnerContainer.SetActiveWeapon(this);
            }
        }
    }

    /// <summary>
    /// Maximum range at which this weapon can do an attack.
    /// </summary>
    public float WeaponRange { get; protected set; }

    public float BaseAttackPower { get; protected set; }

    public MonoBehaviour AttackTimerKiller { get; protected set; }

    public WeaponContainer OwnerContainer { get; protected set; }

    public Weapon(Transform weaponObjectIn, WeaponType weaponTypeIn)
    {
        CreateUniqueWeaponID();
        Weapon_Attacks = new Dictionary<int, A_Attack>();
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        Name = WeaponGameObject.name.Substring(7);
        SoulsTaken = 0;
        BaseAttackPower = 100f;
    }

    public Weapon(WeaponContainer containerIn, MonoBehaviour attackTimerKillerIn, Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn, string nameIn, int soulsIn, float powerIn)
    {
        CreateUniqueWeaponID();
        Weapon_Attacks = new Dictionary<int, A_Attack>();
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        WeaponRange = rangeIn;
        Name = nameIn;
        SoulsTaken = soulsIn;

        BaseAttackPower = powerIn;

        AttackTimerKiller = attackTimerKillerIn;

        OwnerContainer = containerIn;
    }

    public bool DoAttack()
    {
        if (!Attacking)
        {
            ActiveAttack.DoAttack();
            return true;
        }
        return false;
    }

    public bool DoAttack(int attackID)
    {
        if (!Attacking)
        {
            A_Attack att = null;
            if (Weapon_Attacks.TryGetValue(attackID, out att))
            {
                SetActiveAttack(att);
                att.DoAttack();
                return true;
            }        
        }
        return false;
    }

    public void AddAttack(int idIn, A_Attack attackIn)
    {
        if (ActiveAttack == null)
            SetActiveAttack(attackIn);
        Weapon_Attacks.Add(idIn, attackIn);
    }

    public void SetActiveAttack(A_Attack attackIn)
    {
        ActiveAttack = attackIn;
    }

    public bool SetAutoAttackByID(int attackIDin)
    {
        A_Attack att = null;
        bool success = Weapon_Attacks.TryGetValue(attackIDin, out att);
        if (success)
        {
            ActiveAttack = att;
            return true;
        }
        return false;
    }

    public A_Attack GetAttackByID(int attackIDin)
    {
        A_Attack att = null;
        if (Weapon_Attacks.TryGetValue(attackIDin, out att))
        {
            return att;
        }
        return null;
    }

    public abstract Collider[] AffectedColliders();

    private static int ID = 0;
    protected void CreateUniqueWeaponID()
    {
        Unique_ID = ID++;
    }
}
