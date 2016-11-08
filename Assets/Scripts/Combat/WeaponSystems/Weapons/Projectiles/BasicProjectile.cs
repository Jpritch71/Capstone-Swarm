using UnityEngine;
using System.Collections;

public class BasicProjectile : MonoBehaviour, I_Projectile
{
    public int AttackingLayer { get; set; }
    //USED FOR ROCKET, BULLET
    //PARAMS: Damage, Velocity
    public float Velocity { get; protected set; }    
    public float Damage { get; protected set; }
    public Vector3 Direction { get; protected set; }
    public Vector3 Loc
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }
    protected Vector3 target;

    protected GameObject explosion, sparks, halo;

    protected Rigidbody thruster;
    protected Renderer render;
    protected TrailRenderer trail;
    protected static Vector3 direction;
    protected static Quaternion rot;

    protected SphereCollider detonateCollider;

    protected GameObject g;
    protected ParticleSystem parts;

    protected float startingGravityScale;

    protected bool alive = false;

    // Use this for initialization
    void Awake () 
    {
        thruster = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();  

        sparks = transform.Find("GunSparks").gameObject;        
        halo = transform.Find("Halo").gameObject;
        explosion = transform.Find("ExplosionParticles").gameObject;
        detonateCollider = GetComponent<SphereCollider>();
        trail = transform.Find("Trail_Smoke").gameObject.GetComponent<TrailRenderer>();
    }

    /*public virtual void Init(float velocityIn, float damageIn)
    {
        Velocity = velocityIn;
        Damage = damageIn;
    }*/

    public virtual void Init(object[] paramsIn)
    {
        AttackingLayer = (int)Flags.Player;
        Damage = System.Convert.ToSingle(paramsIn[0]);
        Velocity = System.Convert.ToSingle(paramsIn[1]);
    }

    public virtual void Launch(Vector3 targetIn)
    {
        StopAllCoroutines();
        alive = true;
        //detonateTimer = Timer.Register(15f, () => Detonate(), null, this);
        StartCoroutine(_Launch(targetIn));
    }

    public void Launch(Vector3 targetIn, Flags ignoreLayer)
    {
        AttackingLayer = (int)ignoreLayer;
        Launch(targetIn);
    }

    public void Launch(Vector3 targetIn, int ignoreLayer)
    {
        AttackingLayer = ignoreLayer;
        Launch(targetIn);
    }

    protected virtual IEnumerator _Launch(Vector3 targetIn)
    {
        yield return new WaitForEndOfFrame();
        #region LaunchAndVectoring
        thruster.isKinematic = false; //make rigidbody physical
        //thruster.gr = startingGravityScale;
        //direction = targetIn;
        direction = targetIn - transform.position;
        direction.Normalize();
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rot = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.rotation = rot;
        thruster.AddForce(direction.normalized * Velocity, ForceMode.Impulse);
        #endregion

        halo.SetActive(true); //turn on light
        render.enabled = true;

        var e = explosion.GetComponent<ParticleSystem>().emission;
        explosion.GetComponent<ParticleSystem>().Stop(true);


        sparks.GetComponent<ParticleSystem>().Play();
        sparks.transform.parent = null;
        for (int x = 0; x < transform.childCount; ++x)
        {
            g = transform.GetChild(x).gameObject;
            if (g.name.ToLower().Contains("thrust"))
            {
                parts = g.GetComponent<ParticleSystem>();
                e = parts.emission;
                e.enabled = true;
                parts.Clear();
            }
            if (g.name.ToLower().Contains("p_"))
            {
                parts = g.GetComponent<ParticleSystem>();
                e = parts.emission;
                e.enabled = true;
                parts.Clear();
            }
        }
        //Debug.Break();
    }
	
	// Update is called once per frame
	void Update () 
    {
        Debug.DrawRay(target, Vector3.up * .01f, Color.green);
        if (!alive)
            return;
        direction = (transform.position + new Vector3(thruster.velocity.x, thruster.velocity.y, thruster.velocity.z) * Time.deltaTime) - transform.position;
        direction.Normalize();
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(rotation, Vector3.forward);
        thruster.AddForce(Vector3.up, ForceMode.Acceleration);
        transform.rotation = q;
    }

    public virtual void ActivateDamage()
    {
        //spawn a damage object at this location
        //damage objects will handle finding an enemy to damage and applying the damage
        //DfTDamage damn = 
        //damn.Damaging = true;
        DamageControl.Damage_Area damn = new DamageControl.Damage_Area(transform.position, Damage, detonateCollider.radius * 3f);
    }
  
    protected virtual void Detonate()
    {
        StopAllCoroutines();
        ActivateDamage();
        DestroySelf();
        //print("detonate");
    }

    public virtual void DestroySelf()
    {
        alive = false;
        #region Immediate   
        var e = explosion.GetComponent<ParticleSystem>().emission;        
        
        explosion.GetComponent<ParticleSystem>().Play();
        //e.enabled = true;

        for (int x = 0; x < transform.childCount; ++x)
        {
            g = transform.GetChild(x).gameObject;
            //print(g.name.ToLower());
            if (g.name.ToLower().Contains("thrust"))
            {
                parts = g.GetComponent<ParticleSystem>();
                parts.Clear();           
                e = parts.emission;
                e.enabled = false;
            }
            if (g.name.ToLower().Contains("p_"))
            {
                parts = g.GetComponent<ParticleSystem>();
                e = parts.emission;
                e.enabled = false;
            }
        }

        render.enabled = false;
        thruster.velocity = Vector3.zero;
        thruster.ResetInertiaTensor();
        thruster.isKinematic = true;
        //startingGravityScale = thruster.gravityScale;
        //thruster.gravityScale = 0f;

        sparks.transform.parent = transform;
        sparks.transform.localPosition = new Vector3(0, 0, 0);
        #endregion

        #region Timers
        StartCoroutine(ExecuteAfterXSeconds(.5f, () => { halo.SetActive(false); }));

        StartCoroutine(ExecuteAfterXSeconds(10f, () =>
        {
            p_Clear();
            transform.position = new Vector3(-99, -99, -99);
            trail.Clear();
        }));        
        #endregion
    }

    public void Set_OffState()
    {
        alive = false; 
        var e = explosion.GetComponent<ParticleSystem>().emission;

        for (int x = 0; x < transform.childCount; ++x)
        {
            g = transform.GetChild(x).gameObject;
            //print(g.name.ToLower());
            if (g.name.ToLower().Contains("thrust"))
            {
                parts = g.GetComponent<ParticleSystem>();
                parts.Clear();
                e = parts.emission;
                e.enabled = false;
            }
            if (g.name.ToLower().Contains("p_"))
            {
                parts = g.GetComponent<ParticleSystem>();
                e = parts.emission;
                e.enabled = false;
            }
        }

        render.enabled = false;
        thruster.velocity = Vector3.zero;
        //thruster.inertia = 0f;
        thruster.isKinematic = true;
        //startingGravityScale = thruster.gravityScale;
        //t//hruster.gravityScale = 0f;

        sparks.transform.parent = transform;
        sparks.transform.localPosition = new Vector3(0, 0, 0);

        halo.SetActive(false); 

        transform.position = new Vector3(-99, -99,-99);
        trail.Clear();
        p_Clear();
    }

    void OnTriggerEnter(Collider coll)
    {
        int bitwise = (1 << coll.gameObject.layer) & AttackingLayer;
        
        if (bitwise != 0) //if 0, the other collider is not on a layer we are interested in (Enemy, player, obstacle)
            return;

        if (alive)
            Detonate();

        //Debug.Log(bitwise + "<--- bitwise, collider: " + coll.gameObject.name);
    }

    public GameObject _GameObject { get { return gameObject; } }

    protected void p_Clear()
    {
        _GameObject.SetActive(false);
    }

    protected IEnumerator ExecuteAfterXSeconds(float secondsIn, System.Action actionIn)
    {
        yield return new WaitForSeconds(secondsIn);

        actionIn();
    }

    protected IEnumerator Execute_X_TimesOverInterval_Y(float secondsIn, int countIn, System.Action<int> actionIn)
    {
        while (countIn > 0)
        {
            actionIn(countIn);
            --countIn;

            yield return new WaitForSeconds(secondsIn);          
        }
    }
}
