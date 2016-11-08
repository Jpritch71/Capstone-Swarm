using UnityEngine;
using System.Collections;

public class MissileAcel : BasicProjectile
{
    protected float accel;
    // Use this for initialization
    void Start()
    {
        thruster = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();

        sparks = transform.Find("GunSparks").gameObject;
        halo = transform.Find("RocketHalo").gameObject;
        explosion = transform.Find("ExplosionParticles").gameObject;
        detonateCollider = GetComponent<SphereCollider>();
    }

    /* public override void Init(float velocityIn, float damageIn, float accelIn)
     {
         base.Init(velocityIn, damageIn);
         accel = accelIn;
     }*/

    public override void Init(object[] paramsIn)
    {
        base.Init(paramsIn);
        accel = System.Convert.ToSingle(paramsIn[2]);      
    }

    // Update is called once per frame
    void Update ()
    {
        if (!alive)
            return;
        direction = (transform.position + new Vector3(thruster.velocity.x, thruster.velocity.y, 0f) * Time.deltaTime) - transform.position;
        direction.Normalize();
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.rotation = q;
        thruster.AddForce(accel * direction);
    }

    void OnTriggerEnter(Collider coll)
    {
        if (alive)
            Detonate();
    }

    public override void ActivateDamage()
    {
        //spawn a damage object at this location
        //damage objects will handle finding an enemy to damage and applying the damage
        //DfTDamage damn = 
        //damn.Damaging = true;
        print(Damage);
        DamageControl.Damage_Area damn = new DamageControl.Damage_AreaForTime(new Vector2(transform.position.x, transform.position.y), Damage, detonateCollider.radius, 500);
    }
}
