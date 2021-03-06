﻿using UnityEngine;
using System.Collections;
using System;

public class OrkEntity : TaggableEntity
{
    public OrkEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {
        Killable = true;
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
        C_MonoBehavior.DestroySelf();
        Score._Instance.AddScore((int)BaseIntegrity);
    }

    public override void UpdateAction()
    {
        Integrity += .01f;
    }
}