using UnityEngine;
using Combat;

public interface I_Weapon
{
    Vector3 AttackPoint { get; }
    string Name { get; }
    int SoulsTaken { get; }
    float WeaponRange { get; }
    WeaponType TypeOfWeapon { get; }

    Collider[] AffectedColliders();
}
