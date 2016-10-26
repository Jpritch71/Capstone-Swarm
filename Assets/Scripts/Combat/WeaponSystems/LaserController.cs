using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

public class LaserController : MonoBehaviour
{
    protected Stopwatch touchWatch;

    protected Stopwatch laserWatch;
    protected LineRenderer LaserRenderer;
    protected float laserWidth, initialLaserWidth;
    protected bool shrink = true;
    protected GameObject LaserTurret, LaserPoint, LaserPointTarget;
    protected ParticleSystem laserPointParticles, laserPointTargetParticles;

    protected float trackingTime;
    protected float turnSpeed;
    protected int damageTime;
    //protected bool targetEngaged = false;
    public int DamageTime
    {
        get
        {
            return damageTime;
        }
    }
    protected bool attacking = false;
    protected Action alignAction;

    protected List<ColliderSortable> colSortList;

    protected Vector2 inputDownPos, inputUpPos;
    protected bool inputDown = false;

    protected GameObject curTarget;
    protected List<GameObject> targets;
    
    void Start()
    {
        targets = new List<GameObject>();
        touchWatch = new Stopwatch();
        laserWatch = new Stopwatch();

        LaserRenderer = GetComponentInChildren<LineRenderer>(); LaserRenderer.SetVertexCount(2);
        laserWidth = .1f;
        initialLaserWidth = laserWidth;
        LaserRenderer.SetWidth(laserWidth, laserWidth);

        LaserTurret = transform.Find("LaserTurret").gameObject;

        LaserPoint = LaserTurret.transform.Find("LaserPoint").gameObject;
        laserPointParticles = LaserPoint.GetComponent<ParticleSystem>();
        LaserPointTarget = LaserPoint.transform.Find("LaserPointTarget").gameObject;
        laserPointTargetParticles = LaserPointTarget.GetComponent<ParticleSystem>();

        var e = laserPointParticles.emission;
        e.enabled = false;
        e = laserPointTargetParticles.emission;
        e.enabled = false;

        LaserRenderer.SetVertexCount(2);
        turnSpeed = 5f;
        damageTime = 250;
        inputDownPos = inputUpPos = new Vector3(-999, -999, -999);

        alignAction = LaserSightsAlign;
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAction();    
    }

    protected virtual void UpdateAction()
    {
        CheckTarget();
        #region TargetHandling
        if (CurrentTarget != null)
        {
            RotateToTarget();
            if (MissedTarget())
                CurrentTarget = null;
        }
        #endregion  

        if (laserWatch.ElapsedMilliseconds > damageTime)
        {
            LaserOff();
            DamageTarget();
        }
        else if (CurrentTarget != null && attacking)
        {
            alignAction();
        }

    }

    protected void RotateToTarget()
    {
        if (CurrentTarget == null)
            return;    
        Vector3 direction = CurrentTarget.transform.position - LaserTurret.transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        if (!attacking && PointingAtTarget(rot))
        {
            //AttackTarget();
            BeginCharging();
            return;
        }

        LaserTurret.transform.rotation = Quaternion.Slerp(LaserTurret.transform.rotation, rot, Time.deltaTime * turnSpeed * (trackingTime * 5f));
        trackingTime += Time.deltaTime;
    }

    #region TurretRotation
    protected virtual bool MissedTarget()
    {
        try
        {
            if (false && CurrentTarget.transform.position.y < LaserTurret.transform.position.y + .2f)
            {
                UnityEngine.Debug.Log("Missed");
                return true;
            }
        }
        catch (System.NullReferenceException e)
        {
            UnityEngine.Debug.Log("no target");
        }
        return false;
    }
    protected bool PointingAtTarget(Quaternion rotIn)
    {
        if (Mathf.Abs(rotIn.eulerAngles.z - LaserTurret.transform.rotation.eulerAngles.z) <= UnityEngine.Random.Range(.5f, 2f))
            return true;
        return false;
    }
    #endregion

    #region Target
    public GameObject CurrentTarget
    {
        get { return curTarget; }
        set
        {
            if (value == null)
            {
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
                    TargetAsEntity = curTarget.GetComponent<I_Entity>();
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
                TargetAsEntity = curTarget.GetComponent<I_Entity>();
            }
            else
            {
                trackingTime = 0f;
                var temp = targets[0];
                targets.Remove(targets[0]);
                curTarget = temp;
                TargetAsEntity = curTarget.GetComponent<I_Entity>();
                targets.Add(targetIn);
            }
        }
        else
        {
            if (!targets.Contains(targetIn))
                targets.Add(targetIn);
        }
    }
    protected I_Entity targetEntity;
    protected I_Entity TargetAsEntity
    {
        get
        {
            return targetEntity;
        }
        set
        {
            targetEntity = value;
        }
    }

    protected bool CheckTarget()
    {
        if (CurrentTarget == null || TargetAsEntity.Dead)
        {
            StopAttacking();
            CurrentTarget = null;
            return true;
        }
        return false;
    }
    #endregion

    #region LaserControl
    #region LaserSights
    protected virtual void BeginCharging()
    {
        LaserSightsOn();
        attacking = true;
    }

    protected virtual void LaserSightsOn()
    {
        LaserRenderer.SetVertexCount(2);
        LaserRenderer.SetPosition(0, LaserPoint.transform.position);
        LaserRenderer.SetPosition(1, CurrentTarget.transform.position + new Vector3(0, 0, 1f));
        LaserRenderer.SetWidth(.02f, .02f);

        //Timer.Register(1f + TargetAsEntity.Integrity / 100f, () => { LaserAttackOn(); }, null, this);
        ((TaggableEntity)TargetAsEntity).TagEntity(new LaserTag(LaserOff));
        Timer.Register(.05f + Mathf.Clamp((TargetAsEntity.Integrity / 600f) - .025f, 0f, 1f), () => { CheckTarget();  LaserAttackOn(); }, null, false, false, (MonoBehaviour)TargetAsEntity);
        //print(.25f + (TargetAsEntity.Integrity) / 200f);
    }

    protected virtual void LaserSightsAlign()
    {
        try
        {
            LaserRenderer.SetPosition(0, LaserPoint.transform.position);
            LaserRenderer.SetPosition(1, CurrentTarget.transform.position + new Vector3(0, 0, 1f));
        }
        catch(Exception e)
        {
            print(e.Message);
        }
        LaserPointTarget.transform.position = CurrentTarget.transform.position + new Vector3(0, 0, -1f);
    }
    #endregion

    #region LaserAttack
    protected virtual void LaserAttackOn()
    {
        alignAction = LaserAttackAlign;
        laserWatch.Start();

        LaserRenderer.SetPosition(0, LaserPoint.transform.position);
        LaserRenderer.SetPosition(1, CurrentTarget.transform.position + new Vector3(0, 0, 1f));
        LaserRenderer.SetWidth(.1f, .1f);

        var e = laserPointParticles.emission;
        e.enabled = true;
        e = laserPointTargetParticles.emission;
        e.enabled = true;

        LaserPointTarget.transform.position = CurrentTarget.transform.position + new Vector3(0, 0, -1f);
    }

    protected virtual void LaserAttackAlign()
    {
        float margin = .09f;
        if (laserWidth <= initialLaserWidth - margin / 2f && shrink)
        {
            shrink = false;
        }
        else if (laserWidth > initialLaserWidth + margin)
        {
            shrink = true;
        }
        if (shrink)
        {
            laserWidth = Mathf.Lerp(laserWidth, initialLaserWidth - (margin + .01f), Time.deltaTime * 8f);
            LaserRenderer.SetWidth(laserWidth, laserWidth);
        }
        else
        {
            laserWidth = Mathf.Lerp(laserWidth, initialLaserWidth + (margin + .01f), Time.deltaTime * 8f);
            LaserRenderer.SetWidth(laserWidth, laserWidth);
        }
        LaserRenderer.SetPosition(0, LaserPoint.transform.position);
        LaserRenderer.SetPosition(1, CurrentTarget.transform.position + new Vector3(0, 0, 1f));
        LaserPointTarget.transform.position = CurrentTarget.transform.position + new Vector3(0, 0, -1f);
    }   

    protected virtual void LaserOff()
    {
        laserWatch.Stop();
        laserWatch.Reset();
        LaserRenderer.SetVertexCount(0);

        var e = laserPointParticles.emission;
        e.enabled = false;
        e = laserPointTargetParticles.emission;
        e.enabled = false;           
    }
    #endregion
    #endregion

    protected virtual void DamageTarget()
    {
        if (CurrentTarget == null)
            return;
        try
        {
            //var asteroid = CurrentTarget.GetComponent<Asteroid>();
            //asteroid.DestroySelf(false);
            TargetAsEntity.IncurDamage(TargetAsEntity.Integrity);
            //if (entity.Dead || !attackUntilDead)
            StopAttacking();
        }
        catch (MissingReferenceException e)
        {

        }
    }

    protected void StopAttacking()
    {
        LaserOff();
        CurrentTarget = null;
        alignAction = LaserSightsAlign;
        attacking = false;
    }
}

public class ColliderSortable : System.IComparable<ColliderSortable>
{
    public float distance;
    public GameObject asteroid;
    public ColliderSortable(Collider col, Transform tIn)
    {
        asteroid = col.gameObject;
        distance = Vector3.Distance(asteroid.transform.position, tIn.position);
    }

    public int CompareTo(ColliderSortable other)
    {
        return distance.CompareTo(other.distance);
    }
}

public class LaserTag : I_EntityTag
{
    private Action _laserAction;
    public LaserTag(Action _laserIn)
    {
        _laserAction = _laserIn;
    }

    public void TagAction()
    {
        _laserAction.Invoke();
    }
}



