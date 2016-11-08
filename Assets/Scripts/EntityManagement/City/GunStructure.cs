using UnityEngine;
using System.Collections;

public class GunStructure : PlayerStructure
{
    public GunStructure(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.gameObject.layer == 8)
        {
            IncurDamage(500f);
        }
    }

    public override void DeathAction()
    {
        StructureAlive = false;
        ObeliskManager.CheckStatus();
    }
}
