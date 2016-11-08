using UnityEngine;
using System.Collections.Generic;
using System;

public class TowerController : Initializer, I_Controller, I_LicensedToKill
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

    public GameObject roundModel;

    public I_Entity Target
    {
        get
        {
            I_Entity en = null;
            EntityManager.GetEntityByCollider(CurrentTarget.GetComponent<Collider>(), ref en);
            return en;
        }
    }

    protected override void InitAwake ()
    {
        targets = new List<GameObject>();       
        towerWeapon = GetComponent<WeaponPlatform>();
        towerWeapon.Authorizer = this;

        WeaponStats wep;
        AmmoType round;
        AmmoBay ammo;
        WeaponManager.AmmoTag ammoTag;     

        ammoTag = WeaponManager.AmmoTag.Bullet;
        round.ammoModel = roundModel;// GameObject.Find("PlayerControlledGun").GetComponent<PlayerController>().GetProjectileModel(ammoTag);
        round.projectileType = "basic_projectile";
        ammo = new AmmoBay(round, ammoTag, new object[] { 200f, 35f });
        wep = new WeaponStats(100, 2, .25f, 1.5f, 15f, .051f, true, 1.5f, ammo);
        towerWeapon.AddWeapon_Affix(WeaponManager.WeaponTag.AutoCannon, wep);

        towerWeapon.AddAmmo(1000);

        turretBarrel = transform.Find("TurretPivot");
        projectilePoint = turretBarrel.Find("AttackPoint");

        turnSpeed = 5f;
    }

    protected override void InitStart()
    {
        C_Entity = new PlayerStructure(this, 500f, 1);

        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<TowerDetectionSphere>();
        C_TowerTrackingSphere = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<TowerDetectionSphere>();
        C_TowerTrackingSphere.TrackingRange = 25f;
        SetTargettingRange(25f);
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
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate();

        if (C_Entity.Dead)
            return;

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
            if (Vector3.Distance(projectilePoint.position, CurrentTarget.transform.position) > targetingRange)
            {
                StopAttacking();
                return;
            }
            if (weaponReady)
            {
                //print("start firing");
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
    private Quaternion lookRot;
    protected void RotateToTarget()
    {
        if (CurrentTarget == null)
            return;
        facingDirection = turretBarrel.position - CurrentTarget.transform.position;
        turretBarrel.rotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        if (!attacking && PointingAtTarget())
        {
            ReadyToFire();
            return;
        }
        else
        {
            var a = Vector3.Dot((turretBarrel.transform.position - CurrentTarget.transform.position), turretBarrel.forward);
            var i = 1;
        }
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
    protected bool PointingAtTarget()
    {
        return Vector3.Dot((turretBarrel.transform.position - CurrentTarget.transform.position), turretBarrel.forward) > .9f;
        //if (Mathf.Abs(rotIn.eulerAngles.z - turretBarrel.rotation.eulerAngles.z) <= UnityEngine.Random.Range(.5f, 2f))
        //    return true;
        //return false;
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
                    I_Entity en = null;
                    EntityManager.GetEntityByCollider(curTarget.GetComponent<Collider>(), ref en);
                    TargetVelocityComponent = ((TaggableEntity)en);
                    if (en == null)
                        print("NULL IN THE LIST WHAT THE FUCK");
                    //TargetVelocityComponent = curTarget.GetComponent<I_VelocityComponent>();
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
                I_Entity en = null;
                EntityManager.GetEntityByCollider(curTarget.GetComponent<Collider>(), ref en);
                TargetVelocityComponent = ((TaggableEntity)en);
            }
            else
            {
                trackingTime = 0f;
                var temp = targets[0];
                if (temp == null)
                    print("NULL IN THE LIST WHAT THE FUCK");
                targets.Remove(targets[0]);
                curTarget = temp;
                if (curTarget == null)
                    StopAttacking();
                print("This object doesn't have its shit together: " + curTarget);
                I_Entity en = null;
                EntityManager.GetEntityByCollider(curTarget.GetComponent<Collider>(), ref en);
                TargetVelocityComponent = ((TaggableEntity)en);
                //TargetVelocityComponent = curTarget.GetComponent<I_VelocityComponent>(); //missing ref exception here }}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}
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
                targetLoc = CurrentTarget.transform.position;
                return targetLoc;
                //projectileVelocity = towerWeapon.LoadedWeapon.ammo.PeakNextRound().Velocity;
                //_P = projectileVelocity * facingDirection.normalized;
                //_V = TargetVelocityComponent.Velocity * (Vector3.Distance(turretBarrel.position, CurrentTarget.transform.position) / projectileVelocity);


                //Debug.DrawLine(CurrentTarget.transform.position, targetLoc, Color.green, .1f);
                //targetLoc = CurrentTarget.transform.position - _V;
                //return targetLoc;
            }
            else
                return targetLoc;
        }
    }

    public int EntityLayer
    {
        get
        {
            return (int)Flags.Player;
        }
    }

    public GroupUnitMovement C_Movement
    {
        get
        {
            return null;
        }
    }

    public I_Sight C_TowerTrackingSphere { get; protected set; }

    public I_Entity C_Entity
    {
        get; protected set;
    }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }

    protected bool CheckTarget()
    {
        if ((CurrentTarget == null) && attacking)//) || TargetVelocityComponent.IsDead) && attacking)
        {
            if (TargetVelocityComponent == null)
                return false;
            StopAttacking();
            return true;
        }
        return false;
    }
    #endregion   

    protected int trackedTargets = 0;
    void OnTriggerEnt1er(Collider other)
    {
        print("enter");
        int bitwise = (1 << other.gameObject.layer) & ((int)Flags.Enemy);
        if (bitwise == 0)
        {
            print("asteroid in");
            //var t = GetComponent<TaggableEntity>();
            //if (t != null)
            //{
            //    trackedTargets++;
            //    if (trackedTargets == 1)
            //    {
            //        //StopCoroutine(ScanForTargets());
            //        //StartCoroutine(ScanForTargets());
            //    }                
            //    new T_TowerTracking(this, t);
            //}
            CurrentTarget = other.gameObject;
        }
    }

    public void StopTrackingTarget(TaggableEntity entityIn)
    {

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

    public void LoadStats()
    {
        throw new NotImplementedException();
    }
}

public class T_TowerTracking : I_EntityTag
{
    private TowerController trackingTower;
    private TaggableEntity entity;

    public T_TowerTracking(TowerController controllerIn, TaggableEntity entityIn)
    {
        entity = entityIn;
        trackingTower = controllerIn;
        entityIn.TagEntity(this);
    }

    public void TagAction()
    {
        trackingTower.StopTrackingTarget(entity);
    }
}
