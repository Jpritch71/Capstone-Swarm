using UnityEngine;
using System.Collections;

public class BasicProjectile : MonoBehaviour, I_Projectile
{
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

    protected Rigidbody2D thruster;
    protected SpriteRenderer spriteRenderer;
    protected TrailRenderer trail;
    protected static Vector2 direction;
    protected static Quaternion rot;

    protected SphereCollider detonateCollider;

    protected GameObject g;
    protected ParticleSystem parts;

    protected float startingGravityScale;

    protected bool alive = false;

    // Use this for initialization
    void Awake () 
    {
        thruster = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();  

        sparks = transform.Find("GunSparks").gameObject;        
        halo = transform.Find("RocketHalo").gameObject;
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

    protected virtual IEnumerator _Launch(Vector3 targetIn)
    {
        yield return new WaitForEndOfFrame();
        #region LaunchAndVectoring
        thruster.isKinematic = false; //make rigidbody physical
        thruster.gravityScale = startingGravityScale;
        direction = targetIn;
        direction = new Vector3(direction.x, direction.y, transform.position.z) - transform.position;
        direction.Normalize();
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rot = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.rotation = rot;
        thruster.AddForce(direction.normalized * Velocity, ForceMode2D.Impulse);
        #endregion

        halo.SetActive(true); //turn on light
        spriteRenderer.color = Color.white; //make sprite visible 

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
        Debug.DrawRay(target, Vector2.up * .01f, Color.green);
        if (!alive)
            return;
        direction = (transform.position + new Vector3(thruster.velocity.x, thruster.velocity.y, 0f) * Time.deltaTime) - transform.position;
        direction.Normalize();
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.rotation = q;
    }

    public virtual void ActivateDamage()
    {
        //spawn a damage object at this location
        //damage objects will handle finding an enemy to damage and applying the damage
        //DfTDamage damn = 
        //damn.Damaging = true;
        DamageControl.Damage_Area damn = new DamageControl.Damage_Area(new Vector2(transform.position.x, transform.position.y), Damage, detonateCollider.radius);
    }
  
    protected virtual void Detonate()
    {
        StopAllCoroutines();
        ActivateDamage();
        DestroySelf(); 
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
       
        spriteRenderer.color = Color.clear;
        thruster.velocity = Vector2.zero;
        thruster.inertia = 0f;
        thruster.isKinematic = true;
        startingGravityScale = thruster.gravityScale;
        thruster.gravityScale = 0f;

        sparks.transform.parent = transform;
        sparks.transform.localPosition = new Vector2(0, 0);
        #endregion

        #region Timers
        StartCoroutine(ExecuteAfterXSeconds(.5f, () => { halo.SetActive(false); }));

        StartCoroutine(ExecuteAfterXSeconds(10f, () =>
        {
            p_Clear();
            transform.position = new Vector2(-99, -99);
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

        spriteRenderer.color = Color.clear;
        thruster.velocity = Vector2.zero;
        thruster.inertia = 0f;
        thruster.isKinematic = true;
        startingGravityScale = thruster.gravityScale;
        thruster.gravityScale = 0f;

        sparks.transform.parent = transform;
        sparks.transform.localPosition = new Vector2(0, 0);

        halo.SetActive(false); 

        transform.position = new Vector2(-99, -99);
        trail.Clear();
        p_Clear();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (alive)
            Detonate();
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
