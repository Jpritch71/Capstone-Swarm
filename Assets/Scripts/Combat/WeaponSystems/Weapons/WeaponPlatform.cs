using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class WeaponPlatform : MonoBehaviour 
{
    public Dictionary<WeaponManager.WeaponTag, WeaponStats> PlatformWeapons { get; private set; }
    //protected AmmoBay loadedAmmo;
    public WeaponStats LoadedWeapon { get { return loadedWeapon; } protected set { loadedWeapon = value; } }
    protected WeaponStats loadedWeapon; //current weapon configuration for the platform
    protected I_Projectile READY_SHOT; //next projectile to be fired
    public int ammoCount 
    {
        get
        {
            return loadedWeapon.ammo.ammoInBay;
        }
    }

    #region turretControl
    protected GameObject gunBarrel, projectilePoint, g; 
    protected Vector3 direction;
    protected Quaternion rot;
    #endregion

    protected Action<I_LicensedToKill> launchAction;
    public I_LicensedToKill Authorizer { get; set; }
    protected int burstShotsFired = 0, roundsFromClipFired = 0;
    protected bool firing = false;
    protected Action ceaseFireAction;
    protected bool onBurstCoolDown = false; //REVISIT, Potentially now redundant. ---revisted, fuck it. Future Joey, your move.
    protected bool burstFiring = false;
    protected bool onCoolDown = false;
    protected bool reloading = false;
    protected bool canFire
    {
        get
        {
            return !(onCoolDown || reloading || burstFiring || onBurstCoolDown);
        }
    }
    #region Timers;
    protected Timer reloadTimer;
    protected Timer burstTimer;
    protected Timer burstCoolTimer;
    protected Timer coolDownTimer;
    #endregion

    protected WeaponManager.WeaponTag gunType;

    void Awake () 
    {
        gunBarrel = transform.Find("TurretPivot").gameObject;
        projectilePoint = gunBarrel.transform.Find("AttackPoint").gameObject;

        //AffixWeapon(WeaponManager.GunTag.AutoCannon);
        //activeAmmo = BaseBullet;
        launchAction = BurstLaunch;        
    }

	// Update is called once per frame
	void FixedUpdate () 
    {
        //if(canFire)
        //    RotateBarrel();
        FiringLoop();
	}

    #region WeaponLoading
    public void AddWeapon(WeaponManager.WeaponTag typeIn, WeaponStats statsIn)
    {    
        if (PlatformWeapons == null)
        {
            //print("creating weapon list");
            PlatformWeapons = new Dictionary<WeaponManager.WeaponTag, WeaponStats>();
        }
        //print("Adding Weapon tagged as [" + typeIn + "]");
        PlatformWeapons.Add(typeIn, statsIn);
    }

    public void AddWeapon_Affix(WeaponManager.WeaponTag typeIn, WeaponStats statsIn)
    {
        AddWeapon(typeIn, statsIn);
        AffixWeapon(typeIn);
    }

    public void AffixWeapon(WeaponManager.WeaponTag tagIn)
    {
        WeaponStats wep;
        if (!PlatformWeapons.TryGetValue(tagIn, out wep))
        {
            print("Weapon Failed to Load.");
            return;
        }

        if (reloading)
            phase_EndReloadAction = () => { AffixAction(wep); };
        else
            AffixAction(wep);
        
    }

    private void AffixAction(WeaponStats wepIn)
    {
        loadedWeapon = wepIn;
        //loadedWeapon.ammo.ActivateRounds();
        burstShotsFired = 0; roundsFromClipFired = 0;
        if (loadedWeapon.firingAction != null)
        {
            launchAction = loadedWeapon.firingAction;
        }
        else if (loadedWeapon.burstShotCoolDown <= 0f)
        {
            launchAction = ScatterLaunch;
        }
        else if (loadedWeapon.burstCount > 1)
        {
            launchAction = BurstLaunch_Start;
        }
        else if (loadedWeapon.burstCount <= 1)
        {
            launchAction = SingleLaunch;
        }
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }
    #endregion

    public WeaponStats GetPlatformWeapon_ByTag(WeaponManager.WeaponTag tagIn)
    {
        WeaponStats wep;
        if (!PlatformWeapons.TryGetValue(tagIn, out wep))
        {
            return wep;
        }
        return null;
    }

    public WeaponStats GetPlatformWeapon()
    {
        return loadedWeapon;
    }

    #region AmmoHandling
    public void AddAmmo(int x)
    {
        loadedWeapon.ammo.AddAmmo(x);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }    

    public void AddAmmo_BATCH()
    {
        AddAmmo(loadedWeapon.clipSize);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }

    public void AddAmmo_BATCH(WeaponManager.WeaponTag tagIn)
    {
        WeaponStats wep;
        if (!PlatformWeapons.TryGetValue(tagIn, out wep))
        {
            wep.ammo.AddAmmo(wep.clipSize);
        }
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }

    public void AddAmmo_BATCH(WeaponStats wep)
    {
        wep.ammo.AddAmmo(wep.clipSize);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }

    public void AddAmmo_BATCHES(int x)
    {
        AddAmmo(loadedWeapon.clipSize * x);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }
    
    public void AddAmmo_BATCHES(WeaponManager.WeaponTag tagIn, int x)
    {
        WeaponStats wep;
        if (!PlatformWeapons.TryGetValue(tagIn, out wep))
        {
            wep.ammo.AddAmmo(wep.clipSize * x);
        }
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }

    public void AddAmmo_BATCHES(WeaponStats wep, int x)
    {
        wep.ammo.AddAmmo(wep.clipSize * x);
        //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
    }

    protected void OutOfAmmo()
    {
        //print("Out of Ammo");
    }
    #endregion 

    #region FiringRoutines
    public void StartFiring(Action actIn)
    {
        if (!canFire && !firing)
        {
            if (ammoCount > 0 && onBurstCoolDown)
                phase_EndCoolDownAction = () => { firing = true; };
        }
        else
        {
            firing = true;
            ceaseFireAction = actIn;
        }
    }

    public void StartFiring()
    {
        StartFiring(null);
    }

    public void StopFiring()
    {
        if(ceaseFireAction != null)
            ceaseFireAction.Invoke();
        firing = false;
    }

    protected virtual void FiringLoop()
    {
        if(firing)
        {
            //print("firing");
            if (canFire)
            {
                //if (loadedWeapon.burstCount <= 1)
                //{
                //    //print("non burst");
                //    //Launch(Authorizer.TargetLoc);
                //    //Launch(TurretControl.inputPos);
                //    SingleLaunch
                //    onCoolDown = true;
                //    Timer.Register(loadedWeapon.coolDown, () => { onCoolDown = false; }, null, false, false, this);
                //}
                //else if (loadedWeapon.burstCount > 1)
                //{
                //    launchAction(Authorizer);
                //}
                if (!loadedWeapon.autoFire)
                    StopFiring();
                launchAction(Authorizer);
            }
        }
    }

    protected void BurstLaunch_Start(I_LicensedToKill weaponAuthorizer)
    {
        burstFiring = true;
        BurstLaunch(weaponAuthorizer);
    }

    protected void BurstLaunch(I_LicensedToKill weaponAuthorizer)
    {
        if (ammoCount > 0)
        {
            Launch(weaponAuthorizer.TargetLoc);
            onBurstCoolDown = true;

            burstShotsFired++;
            if (burstShotsFired > loadedWeapon.burstCount - 1)
            {
                burstFiring = false;
                onCoolDown = true;
                coolDownTimer = Timer.Register(loadedWeapon.coolDown,
                                                                       () =>
                                                                       {
                                                                           onBurstCoolDown = false; EndCoolDown(); burstShotsFired = 0;
                                                                       }, null, false, false, this);

            }
            else
            {
                burstTimer = Timer.Register(loadedWeapon.burstShotCoolDown,
                                                                            () =>
                                                                            {
                                                                                BurstLaunch(weaponAuthorizer);
                                                                                onBurstCoolDown = false;
                                                                            }, null, false, false, this);
            }
        }
        else
            OutOfAmmo();
    }

    protected void ScatterLaunch(I_LicensedToKill weaponAuthorizer)
    {
        if (ammoCount > 0)
        {
            print("burst count: " + loadedWeapon.burstCount);
            for (int x = 0; x < loadedWeapon.burstCount; ++x)
                Launch(weaponAuthorizer.TargetLoc);
            Timer.Register(loadedWeapon.coolDown, () => { EndCoolDown(); }, null, false, false, this);
            onCoolDown = true;
        }
        else
            OutOfAmmo();
    }


    protected void SingleLaunch(I_LicensedToKill weaponAuthorizer)
    {
        if (ammoCount > 0)
        {
            onCoolDown = true;
            Launch(weaponAuthorizer.TargetLoc);
            Timer.Register(loadedWeapon.coolDown, () => { EndCoolDown(); }, null, false, false, this);
        }
        else
            OutOfAmmo();
    }

    public void Launch(Vector3 targetIn)
    {
        if (ammoCount > 0 && roundsFromClipFired < loadedWeapon.clipSize)
        {
            roundsFromClipFired++;
            float f = GetFallOffSpread(targetIn);
            targetIn = new Vector3(targetIn.x + UnityEngine.Random.Range(-f, f), targetIn.y + UnityEngine.Random.Range(-f, f), targetIn.z + UnityEngine.Random.Range(-f, f));                        

            READY_SHOT = loadedWeapon.ammo.ExpendNextRound();
            READY_SHOT.Loc = projectilePoint.transform.position;
            READY_SHOT.Launch(targetIn);
            //SARSiteUI._Instance.ammoString = "Ammo: " + ammoCount;
        }
        else if(roundsFromClipFired >= loadedWeapon.clipSize)
        {
            //print("reload");
            StartReload();
        }
        else if(ammoCount <= 0)
        {
            OutOfAmmo();
        }
    }

    public void StartReload()
    {
        reloading = true;
        if(burstTimer != null)
            burstTimer.Cancel();
        if (burstCoolTimer != null)
            burstCoolTimer.Cancel();
        if (coolDownTimer != null)
            coolDownTimer.Cancel();
        reloadTimer = Timer.Register(loadedWeapon.reloadTime,
                                     () =>
                                     {
                                         FinishReload();
                                     }, null, false, false, this);
    }

    protected virtual float GetFallOffSpread(Vector3 targetLocIn)
    {
        return 1 - Mathf.Clamp(1 - loadedWeapon.fallOffFactor * (Vector3.Distance(transform.position, targetLocIn) - loadedWeapon.fallOffDistance), 0, 1);
    }
    #endregion

    #region PhaseActions
    protected Action phase_EndCoolDownAction = null;
    protected void EndCoolDown()
    {
        onCoolDown = false;
        if (phase_EndCoolDownAction != null)
        {
            phase_EndCoolDownAction.Invoke();
        }
        phase_EndCoolDownAction = null;
    }

    protected Action phase_EndReloadAction = null;
    protected void FinishReload()
    {
        reloading = false;
        onBurstCoolDown = false; onCoolDown = false;
        reloading = false; burstShotsFired = 0;
        roundsFromClipFired = 0;
        if (phase_EndReloadAction != null)
        {
            phase_EndReloadAction.Invoke();
        }
        phase_EndReloadAction = null;
    }
    #endregion
}

public class WeaponManager
{
    //Tag associated with the the players weapons. 
    //Players may update each type of weapon individually. 
    public enum WeaponTag
    {
        MissileLauncher, AutoCannon, RocketLauncher, ScatterGun
    }
    public Dictionary<string, WeaponPlatform> playerGuns;

    public enum AmmoTag
    {
        Rocket, Missile, Bullet, Lance
    }
    //protected Dictionary<AmmoTag, AmmoBay> weaponAmmos;

    public WeaponPlatform currentWeapon { get; private set; }

    private static WeaponManager instance;
    private WeaponManager()
    {
        playerGuns = new Dictionary<string, WeaponPlatform>();
        //weaponAmmos = new Dictionary<AmmoTag, AmmoBay>();
    }

    public static WeaponManager _Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WeaponManager();
            }
            return instance;
        }
    }

    public void AddWeapon(string gunKeyIn, WeaponPlatform gunIn)
    {
        playerGuns.Add(gunKeyIn, gunIn);
        if(currentWeapon == null)
        {
            currentWeapon = gunIn;
        }
    }

    public void SetCurrentWeapon(WeaponPlatform gunIn)
    {
        currentWeapon = gunIn;
    }

    public void SetCurrentWeapon(string gunIn)
    {
        WeaponPlatform temp;
        if(playerGuns.TryGetValue(gunIn, out temp))
        {
            currentWeapon = temp;
        }
    }

    public WeaponPlatform GetWeapon(string gunKeyIn)
    {
        WeaponPlatform temp;
        if(playerGuns.TryGetValue(gunKeyIn, out temp))
        {
            return temp;
        }
        else
        {
            return null;
        }
    }
}

public class WeaponStats
{
    public int clipSize;
    public int burstCount;
    public float burstShotCoolDown;
    public float coolDown;
    public float fallOffDistance;
    public float fallOffFactor;
    public bool autoFire;
    public float reloadTime;

    public AmmoBay ammo;

    public Action<I_LicensedToKill> firingAction;

    public WeaponStats(int clipSizeIn, int burstIn, float burstCoolIn, float coolIn,
                        float fallOffDistanceIn, float fallOffFactorIn, bool autoFlagIn, float reloadTimeIn, AmmoBay ammoIn)
    {
        clipSize = clipSizeIn;
        burstCount = burstIn;
        clipSize -= clipSize % burstCount;

        burstShotCoolDown = burstCoolIn;
        coolDown = coolIn;
        fallOffDistance = fallOffDistanceIn;
        fallOffFactor = fallOffFactorIn;
        autoFire = autoFlagIn;

        reloadTime = reloadTimeIn;
        ammo = ammoIn;

        ammo.InitPool(this);

        firingAction = null;
    }
}

public struct AmmoType
{
    public GameObject ammoModel;
    public string projectileType;

    public override string ToString()
    {
        return "[Model Name: (" + ammoModel.name + "), Projectile Type: (" + projectileType.GetType() + ")]";
    }
}

public class AmmoBay 
{
    public AmmoType bayAmmo { get; private set; }
    private WeaponManager.AmmoTag ammoTag;
    private int ammoBayCapacity { get; set; }
    public int ammoInBay { get; protected set; }

    private I_Projectile[] rounds;
    private int nextRound = 0;

    private object[] parameters;

    public bool intitialized { get; private set; }

    public AmmoBay(AmmoType ammoIn, WeaponManager.AmmoTag tagIn, object[] paramsIn)
    {
        bayAmmo = ammoIn;
        ammoTag = tagIn;
        parameters = paramsIn;
    }

    public void InitPool(WeaponStats weaponIn)
    {
        int z = 0;
        rounds = new I_Projectile[(weaponIn.burstCount > 1) ?
                                                            (int)(weaponIn.burstCount / (Mathf.Clamp(weaponIn.burstShotCoolDown, .01f, 50f) * weaponIn.burstCount + weaponIn.coolDown) * 10)
                                                            :
                                                            (int)((1f / ((weaponIn.coolDown) + 0.01f)) * 10f)];
        z = (int)((1f / ((weaponIn.coolDown) + 0.01f)) * 10f);
        //rounds = new Projectile[600];
        GameObject model, g;
        model = GameObject.Instantiate(bayAmmo.ammoModel, new Vector3(-99, -99, -99), Quaternion.identity) as GameObject;
        AddProjectile_ByString(bayAmmo.projectileType, model);

        int y = rounds.Length;

        for (int x = 0; x < rounds.Length; ++x)
        {
            g = GameObject.Instantiate(model, new Vector3(-99, -99, -99), Quaternion.identity) as GameObject;
            rounds[x] = g.GetComponent<I_Projectile>();
            rounds[x].Init(parameters);
            rounds[x].Set_OffState();
        }
        intitialized = true;
        DeactivateRounds();
        ammoBayCapacity = weaponIn.clipSize * 20;
    }

    public void ActivateRounds()
    {
        for (int x = 0; x < rounds.Length; ++x)
        {
            rounds[x]._GameObject.SetActive(true);
        }
    }

    public void DeactivateRounds()
    {
        for (int x = 0; x < rounds.Length; ++x)
        {
            rounds[x]._GameObject.SetActive(false);
        }
    }

    public int AddAmmo(int ammoIn)
    {
        if(ammoInBay + ammoIn > ammoBayCapacity)
        {
            int temp = ammoBayCapacity - ammoInBay;
            ammoInBay = ammoBayCapacity;
            return temp;
        }
        ammoInBay += ammoIn;
        return 0;
    }

    public I_Projectile GetNextRound()
    {
        if (!intitialized)
            return null;
        if (nextRound > rounds.Length - 1)
            nextRound = 0;
        return ActivateRound(rounds[nextRound++]);
    }

    public I_Projectile ActivateRound(I_Projectile projIn)
    {
        projIn._GameObject.SetActive(true);
        return projIn;
    }

    public I_Projectile PeakNextRound()
    {
        return (nextRound > rounds.Length - 1) ? rounds[0] : rounds[nextRound];
    }

    public I_Projectile ExpendNextRound()
    {
        ammoInBay--;
        return GetNextRound();
    }

    //forces the next round in the array to destroy itself
    //this frees the round up for use
    //increases the counter
    public void RemoveNextRound()
    {
        if (nextRound > rounds.Length - 1)
            nextRound = 0;
        rounds[nextRound].Set_OffState();
    }

    private void HandleProjectileInit()
    {
        if (intitialized)
        {
            for (int x = 0; x < rounds.Length - 1; ++x)
            {
                rounds[x].Init(parameters);
            }
        }
    }

    //this objects are solely for type reference.
    #region ProjectileTypes
    public static void AddProjectile_ByString(string typeIn, GameObject g)
    {

        switch (typeIn.ToLower())
        {
            case "basic_projectile":
            case "basic_rocket":
            case "basic_round":
            case "basic":        
                g.AddComponent<BasicProjectile>();
                return;
            case "basic_lance":
                g.AddComponent<LanceProjectile>();
                return;

            case "missile_no_tracking":
                g.AddComponent<MissileAcel>();
                return;

            default:
                g.AddComponent<BasicProjectile>();
                return;
        }
    }
    #endregion
}

/// <summary>
/// Implementing this interface means the class can issue fire commands to a weaponsystem.
/// </summary>
public interface I_LicensedToKill
{
    Vector3 TargetLoc { get; }
}
