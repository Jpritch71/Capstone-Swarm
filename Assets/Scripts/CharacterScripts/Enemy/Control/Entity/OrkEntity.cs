using UnityEngine;
using System.Collections;
using System;

public class OrkEntity : TaggableEntity
{
    public override int EntityLayer { get { return 12; } }
    public OrkEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {

    }

    public override string Name
    {
        get
        {
            return Owner_C_Controller.C_AttachedGameObject.name;
        }
    }

    protected override void DeathWork()
    {
        C_MonoBehavior._MSG("ork death");
    }
}