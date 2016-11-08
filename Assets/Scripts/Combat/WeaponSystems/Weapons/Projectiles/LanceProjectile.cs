using UnityEngine;
using System.Collections;

public class LanceProjectile : BasicProjectile
{
    protected float maxDistance;
    protected Vector3 startPos;
    protected new Vector3 direction;
    protected LineRenderer LaserRenderer;
    protected float startWidth;
    protected float currentWidth;

    public override void Init(object[] paramsIn)
    {
        thruster = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();

        sparks = transform.Find("GunSparks").gameObject;
        halo = transform.Find("RocketHalo").gameObject;
        explosion = transform.Find("ExplosionParticles").gameObject;
        detonateCollider = GetComponent<SphereCollider>();
        LaserRenderer = transform.Find("Line_Smoke").gameObject.GetComponent<LineRenderer>();

        startWidth = .01f; currentWidth = startWidth;

        Damage = System.Convert.ToSingle(paramsIn[0]);
        maxDistance = System.Convert.ToSingle(paramsIn[1]);
        maxDistance = Random.Range(.95f, 1.05f) * maxDistance;
    }

    protected override IEnumerator _Launch(Vector3 targetIn)
    {
        startPos = Loc;
        currentWidth = startWidth;

        print(LaserRenderer);
        LaserRenderer.SetVertexCount(2);
        LaserRenderer.SetPosition(0, startPos);
        LaserRenderer.SetPosition(1, Loc);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        #region LaunchAndVectoring
        thruster.isKinematic = false; //make rigidbody physical
        //thruster.gravityScale = startingGravityScale;
        direction = targetIn;
        direction = new Vector3(direction.x, direction.y, transform.position.z) - transform.position;
        direction.Normalize();

        RaycastHit2D hit = Physics2D.Raycast(Loc, direction, maxDistance, 1 << 8);
        if (hit.collider != null)
        {
            Loc = hit.point;
            LaserRenderer.SetPosition(1, Loc);
            StartCoroutine(ExecuteAfterXSeconds(.00001f, () => { Detonate(); FadeTrail(); }));
        }

        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rot = Quaternion.AngleAxis(rotation, Vector3.forward);
        transform.rotation = rot;
        //thruster.AddForce(direction.normalized * Velocity);
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
            }
            if (g.name.ToLower().Contains("p_"))
            {
                parts = g.GetComponent<ParticleSystem>();
                e = parts.emission;
                e.enabled = true;
            }
        }
        StartCoroutine(ExecuteAfterXSeconds(.1f, () =>
                                                    {
                                                        Loc = Loc + (direction * maxDistance);
                                                        Detonate();
                                                        LaserRenderer.SetPosition(1, Loc);
                                                        FadeTrail();
                                                    }));
    }

    protected virtual void FadeTrail()
    {
        float x = 10;
        StartCoroutine(Execute_X_TimesOverInterval_Y(.24f, (int)x, (c) =>
        {
            Color c1, c2;
            c2 = Color.Lerp(Color.white, Color.clear, 1 - ((c - 1) / x) + Random.Range(.01f, .03f));
            c1 = Color.Lerp(Color.white, Color.clear, 1 - ((c - 1) / x) + Random.Range(.01f, .03f));

            LaserRenderer.SetColors(c1, c2);
            currentWidth += .009f;
            LaserRenderer.SetWidth(currentWidth, currentWidth);
        }));
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        StartCoroutine(ExecuteAfterXSeconds(5f, () =>
                                {
                                    LaserRenderer.SetPosition(0, new Vector2(-99, -99));
                                    LaserRenderer.SetPosition(1, new Vector2(-99, -99));
                                }));
    }

    // Update is called once per frame
    void Update()
    {
        if (!alive)
            return;

        /*if(Vector2.Distance(startPos, Loc) > maxDistance)
        {
            Detonate();
        }*/
    }
}
