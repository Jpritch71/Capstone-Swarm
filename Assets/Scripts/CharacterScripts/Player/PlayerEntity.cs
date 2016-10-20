using UnityEngine;
using System.Collections;
using System;

public class PlayerEntity : TaggableEntity
{
    public PlayerEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn) : base(controllerMonoBehavior, baseIntegrityIn)
    {
    }

    protected override void DeathWork()
    {
        C_MonoBehavior._MSG("Player Dead");
    }
}
