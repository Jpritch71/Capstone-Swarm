using UnityEngine;
using System.Collections;
using Combat;
using System;

public abstract class Weapon : I_Weapon
{
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

    /// <summary>
    /// Maximum range at which this weapon can do an attack.
    /// </summary>
    public float WeaponRange { get; protected set; }

    public Weapon(Transform weaponObjectIn, WeaponType weaponTypeIn)
    {
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        Name = WeaponGameObject.name.Substring(7);
        SoulsTaken = 0;
    }

    public Weapon(Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn, string nameIn, int soulsIn)
    {
        WeaponGameObject = weaponObjectIn;
        TypeOfWeapon = weaponTypeIn;
        weaponAttackPoint = WeaponGameObject.Find("AttackPoint");
        WeaponRange = rangeIn;
        Name = nameIn;
        SoulsTaken = soulsIn;
    }

    public abstract Collider[] AffectedColliders();
}
