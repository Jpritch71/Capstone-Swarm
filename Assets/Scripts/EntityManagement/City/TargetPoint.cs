using UnityEngine;
using System.Collections;
using System;

public class TargetPoint : Entity
{
    protected I_WeakSpot owner;

    private float lastDamage; //whatever damage was most recently inflicted upon the target point

    public override string Name
    {
        get
        {
            return "Target Point ID[" + Unique_ID + "] for : " + Owner_C_Controller.C_AttachedGameObject.name;
        }
    }

    public TargetPoint(I_WeakSpot controllerMonoBehavior)
    {
        Killable = true;
        InitializeIntegrity(1f);
        EntityLevel = 1;
    }

    public override void DeathAction()
    {
        owner.C_OwnerEntity.IncurDamage(lastDamage);
        Integrity = BaseIntegrity;
    }

    public override void IncurDamage(float damageIn)
    {
        if (!Killable)
        {
            return;
        }

        lastDamage = damageIn;

        Integrity -= damageIn;
        if (Integrity <= 0)
        {
            DeathAction();
        }       
    }

    public override void UpdateAction()
    {
    }
}

public interface I_WeakSpot
{
    I_Entity C_OwnerEntity { get; }
}
