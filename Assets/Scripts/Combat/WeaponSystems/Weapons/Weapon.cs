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

    public bool Attacking { get; set; }

    /// <summary>
    /// Maximum range at which this weapon can do an attack.
    /// </summary>
    public float WeaponRange { get; protected set; }

    public Weapon(Transform weaponObjectIn, WeaponType weaponTypeIn)
    {
        CreateUniqueWeaponID();
        Weapon_Attacks = new Dictionary<int, A_Attack>();
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        Name = WeaponGameObject.name.Substring(7);
        SoulsTaken = 0;
    }

    public Weapon(Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn, string nameIn, int soulsIn)
    {
        CreateUniqueWeaponID();
        Weapon_Attacks = new Dictionary<int, A_Attack>();
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        WeaponRange = rangeIn;
        Name = nameIn;
        SoulsTaken = soulsIn;
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
                att.DoAttack();
                return true;
            }        
        }
        return false;
    }

    public void AddAttack(int idIn, A_Attack attackIn)
    {
        if (ActiveAttack == null)
            SetAutoAttack(attackIn);
        Weapon_Attacks.Add(idIn, attackIn);
    }

    public void SetAutoAttack(A_Attack attackIn)
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

    public abstract Collider[] AffectedColliders();

    private static int ID = 0;
    protected void CreateUniqueWeaponID()
    {
        Unique_ID = ID++;
    }
}
