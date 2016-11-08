using UnityEngine;
using Combat;

public class RangedWeapon : Weapon
{
    public WeaponStats weaponStats;
    protected I_Projectile READY_SHOT;

    public RangedWeapon(Transform weaponObjectIn, WeaponType weaponTypeIn) : base(weaponObjectIn, weaponTypeIn)
    {

    }

    public RangedWeapon(WeaponContainer containerIn,WeaponStats wepStatsIn, MonoBehaviour attackTimerKillerIn,
                            Transform weaponObjectIn, WeaponType weaponTypeIn, float rangeIn, string nameIn,
                                int soulsIn, float powerIn)
        : base(containerIn, attackTimerKillerIn, weaponObjectIn, weaponTypeIn, rangeIn, nameIn, soulsIn, powerIn)
    {
        weaponStats = wepStatsIn;
    }

    public override Collider[] AffectedColliders()
    {
        return Physics.OverlapSphere(AttackPoint, WeaponRange, WorldManager.entityFlag, QueryTriggerInteraction.Collide);
    }

    protected virtual float GetFallOffSpread(Vector3 targetLocIn)
    {
        return 1 - Mathf.Clamp(1 - weaponStats.fallOffFactor * (Vector3.Distance(AttackPoint, targetLocIn) - weaponStats.fallOffDistance), 0, 1);
    }

    public void Launch(Vector3 targetIn)
    {
        float f = GetFallOffSpread(targetIn);
        targetIn = new Vector3(targetIn.x + Random.Range(-f, f), targetIn.y + Random.Range(-f, f), targetIn.z + Random.Range(-f, f));

        READY_SHOT = weaponStats.ammo.ExpendNextRound();
        READY_SHOT.Loc = AttackPoint;
        READY_SHOT.Launch(targetIn);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;      
    }

    public void Launch(Vector3 targetIn, Flags ignoreLayer)
    {
        float f = GetFallOffSpread(targetIn);
        targetIn = new Vector3(targetIn.x + Random.Range(-f, f), targetIn.y + Random.Range(-f, f), targetIn.z + Random.Range(-f, f));

        READY_SHOT = weaponStats.ammo.ExpendNextRound();
        READY_SHOT.Loc = AttackPoint;
        READY_SHOT.Launch(targetIn, ignoreLayer);
    }

    public void Launch(Vector3 targetIn, int ignoreLayer)
    {
        float f = GetFallOffSpread(targetIn);
        targetIn = new Vector3(targetIn.x + Random.Range(-f, f), targetIn.y + Random.Range(-f, f), targetIn.z + Random.Range(-f, f));

        READY_SHOT = weaponStats.ammo.ExpendNextRound();
        READY_SHOT.Loc = AttackPoint;
        READY_SHOT.Launch(targetIn, ignoreLayer);
    }
}
