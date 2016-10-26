using UnityEngine;
using System.Collections;
using System;

public class TargetPoint : Entity
{
    public override int EntityLayer { get { return 10; } }
    protected I_Entity parentEntity;
    private float lastDamage; //whatever damage was most recently inflicted upon the target point

    public override string Name
    {
        get
        {
            return "Target Point ID[" + Unique_ID + "] for : " + Owner_C_Controller.C_AttachedGameObject.name;
        }
    }

    public TargetPoint(I_Controller controllerMonoBehavior, float baseIntegrityIn) : base(controllerMonoBehavior, baseIntegrityIn, 1)
    {
        Killable = true;
        InitializeIntegrity(baseIntegrityIn);
        parentEntity = controllerMonoBehavior.C_Entity;
    }
       
    public TargetPoint(I_Controller controllerMonoBehavior) : base(controllerMonoBehavior)
    {
        Killable = true;
        InitializeIntegrity(1f);
        parentEntity = controllerMonoBehavior.C_Entity;
        EntityLevel = 1;
    }

    public override void DeathAction()
    {
        parentEntity.IncurDamage(lastDamage);
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
}
