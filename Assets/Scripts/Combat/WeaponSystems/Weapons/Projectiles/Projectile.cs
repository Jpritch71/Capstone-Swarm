using UnityEngine;
using System.Collections;

public interface I_Projectile 
{
    float Velocity { get; }
    float Damage { get; }
    Vector3 Direction { get; }
    Vector3 Loc { get; set; }

    // void Init(float velocityIn, float damageIn);
    void Init(object[] paramsIn);
    void Launch(Vector3 targetIn);
    void ActivateDamage();
    void DestroySelf();
    void Set_OffState();

    GameObject _GameObject { get; }
}
