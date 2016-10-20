using UnityEngine;
using System.Collections;
using System;

public class OrkEntity : TaggableEntity
{
    public OrkEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn) : base(controllerMonoBehavior, baseIntegrityIn)
    {
    }

    protected override void DeathWork()
    {
        C_MonoBehavior._MSG("Ork Dead");
    }
}
