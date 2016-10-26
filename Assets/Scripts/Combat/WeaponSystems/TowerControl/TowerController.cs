using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour, I_LicensedToKill
{
    protected float targetingRange;
    protected bool targetInTrackingArea = false;
    protected bool attacking = false;
    protected bool weaponReady = true;

    protected float turnSpeed;
    protected float trackingTime;
    protected Vector3 facingDirection, _P, _V, _PonV;
    protected float projectileVelocity;

    protected Transform turretBarrel, projectilePoint;

    WeaponPlatform towerWeapon;

    protected bool active = true;

    void Awake ()
    {
        targets = new List<GameObject>();

        turretBarrel = transform.Find("BARREL");
        projectilePoint = transform.Find("POINT");
        towerWeapon = GetComponent<WeaponPlatform>();
        towerWeapon.Authorizer = this;

        WeaponStats wep;
        AmmoType round;
        AmmoBay ammo;
        WeaponManager.AmmoTag ammoTag;     


        ammoTag = WeaponManager.AmmoTag.Bullet;
        round.ammoModel = null;// GameObject.Find("PlayerControlledGun").GetComponent<PlayerController>().GetProjectileModel(ammoTag);
        round.projectileType = "basic_projectile";
        ammo = new AmmoBay(round, ammoTag, new object[] { 3f, 15f });
        wep = new WeaponStats(100, 1, .01f, .01f, 3f, .051f, true, 1.5f, ammo);
        towerWeapon.AddWeapon_Affix(WeaponManager.WeaponTag.AutoCannon, wep);

        towerWeapon.AddAmmo(1000);

        turretBarrel = transform.Find("Turret");
        projectilePoint = turretBarrel.transform.Find("TargetPoint");

        turnSpeed = 5f;


        SetTargettingRange(15f);
    }

    protected void SetTargettingRange(float rangeIn)
    {
        targetingRange = rangeIn;
        CapTargetingRange();
    }

    protected void CapTargetingRange()
    {
        if(towerWeapon.LoadedWeapon.ammo.bayAmmo.projectileType.ToLower().Contains("lance"))
        {
            targetingRange = Mathf.Clamp(targetingRange, 0, towerWeapon.LoadedWeapon.ammo.PeakNextRound().Velocity * .1f);
        }
    }


    void FixedUpdate()
    {
        if (CurrentTarget != null)
        {
            
        }
        if(active)
            UpdateAction();
    }

    protected virtual void UpdateAction()
    {
        if (CurrentTarget != null && attacking)
        {
            if (weaponReady)
            {
                print("start firing");
                weaponReady = false;
                towerWeapon.StartFiring(() => { weaponReady = true; });
            }
        }

        CheckTarget();
        #region TargetHandling
        if (CurrentTarget != null)
        {
            RotateToTarget();
            if (MissedTarget())
            {
                CurrentTarget = null;
            }
        }
        #endregion         
    }

    #region TurretTracking
    protected void RotateToTarget()
    {
        if (CurrentTarget == null)
            return;
        facingDirection = CurrentTarget.transform.position - turretBarrel.position;

        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle - 90f, Vector3.up   );
        if (!attacking && PointingAtTarget(rot))
        {
            ReadyToFire();
            return;
        }

        turretBarrel.rotation = Quaternion.Slerp(turretBarrel.rotation, rot, Time.deltaTime * turnSpeed * (trackingTime * 5f));
        trackingTime += Time.deltaTime;
    }

    protected void ReadyToFire()
    {        
        attacking = true;
    }

    protected void StopAttacking()
    {
        attacking = false;
        CurrentTarget = null;
        towerWeapon.StopFiring();
    }
    #endregion

    #region TurretRotation
    protected virtual bool MissedTarget()
    {
        if(CurrentTarget != null)
        {         
            if (false)
            {
                StopAttacking();
                return true;
            }
        }
        return false;
    }
    protected bool PointingAtTarget(Quaternion rotIn)
    {
        if (Mathf.Abs(rotIn.eulerAngles.z - turretBarrel.rotation.eulerAngles.z) <= UnityEngine.Random.Range(.5f, 2f))
            return true;
        return false;
    }
    #endregion

    #region Targeting
    protected GameObject curTarget;
    protected List<GameObject> targets;
    public GameObject CurrentTarget
    {
        get { return curTarget; }
        set
        {
            if (value == null)
            {
                TargetLoc = TargetLoc; //assigns there current location of the target - avoids null reference
                if (targets.Count > 0)
                {
                    var temp = targets[0];
                    targets.Remove(targets[0]);
                    if (temp == null)
                    {
                        CurrentTarget = null;
                        return;
                    }
                    curTarget = temp;
                    TargetVelocityComponent = curTarget.GetComponent<I_VelocityComponent>();
                    trackingTime = 0f;
                }
                else
                {
                    curTarget = value;
                }
            }
            else
            {
                SetCurrentTarget(value);
            }
        }
    }
    protected void SetCurrentTarget(GameObject targetIn)
    {
        if (CurrentTarget == null)
        {
            if (targets.Count == 0)
            {
                trackingTime = 0f;
                curTarget = targetIn;
                TargetVelocityComponent = curTarget.GetComponent<I_VelocityComponent>();
            }
            else
            {
                trackingTime = 0f;
                var temp = targets[0];
                if (temp == null)
                    print("NULL IN THE LIST WHAT THE FUCK");
                targets.Remove(targets[0]);
                curTarget = temp;
                print("This object doesn't have its shit together: " + curTarget);
                TargetVelocityComponent = curTarget.GetComponent<I_VelocityComponent>(); //missing ref exception here }}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}
                targets.Add(targetIn);
            }
        }
        else
        {
            if (!targets.Contains(targetIn))
                targets.Add(targetIn);
        }
    }
    protected I_VelocityComponent targetVelocityComponent;
    protected I_VelocityComponent TargetVelocityComponent
    {
        get
        {
            return targetVelocityComponent;
        }
        set
        {
            targetVelocityComponent = value;
        }
    }

    protected Vector3 targetLoc;
    public Vector3 TargetLoc
    {
        set
        {
            targetLoc = value;
        }
        get
        {

            if (CurrentTarget != null && (TargetVelocityComponent != null && !TargetVelocityComponent.IsDead))
            {
                projectileVelocity = towerWeapon.LoadedWeapon.ammo.PeakNextRound().Velocity;
                _P = projectileVelocity * facingDirection.normalized;
                _V = TargetVelocityComponent.Velocity * (Vector3.Distance(turretBarrel.position, CurrentTarget.transform.position) / projectileVelocity);


                Debug.DrawLine(CurrentTarget.transform.position, targetLoc, Color.green, .1f);
                targetLoc = CurrentTarget.transform.position - _V;
                return targetLoc;
            }
            else
                return targetLoc;
        }
    }

    protected bool CheckTarget()
    {
        if ((CurrentTarget == null || TargetVelocityComponent.IsDead) && attacking)
        {
            StopAttacking();
            return true;
        }
        return false;
    }
    #endregion   

    protected int trackedTargets = 0;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            print("asteroid in");
            var t = GetComponent<TaggableEntity>();
            if (t != null)
            {
                trackedTargets++;
                if (trackedTargets == 1)
                {
                    //StopCoroutine(ScanForTargets());
                    StartCoroutine(ScanForTargets());
                }                
                new T_TowerTracking(this, t);
            }
            //CurrentTarget = other.gameObject;
        }
    }

    public void StopTrackingTarget(TaggableEntity entityIn)
    {
        trackedTargets--;

        if (trackedTargets <= 0)
        {
            targetInTrackingArea = false;
        }

        if(trackedTargets < 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }
    }

    Collider[] collidersOnRadar;
    protected System.Collections.IEnumerator ScanForTargets()
    {
        print("IN");
        while(trackedTargets > 0)
        {
            print("IF THIS IS NOT WORKING IT'S BECAUSE YOU FORGOT TO COME BACK AND FIX THE LAYER");
      
            collidersOnRadar = Physics.OverlapSphere(transform.position, targetingRange, (1 << 8));

            foreach(Collider c in collidersOnRadar)
            {
                CurrentTarget = c.gameObject;
            }
            yield return new WaitForSeconds(.15f);
        }
    }
}

public class T_TowerTracking : I_EntityTag
{
    private TowerController trackingTower;
    private TaggableEntity entity;

    public T_TowerTracking(TowerController controllerIn, TaggableEntity entityIn)
    {
        trackingTower = controllerIn;
        entityIn.TagEntity(this);
    }

    public void TagAction()
    {
        trackingTower.StopTrackingTarget(entity);
    }
}
