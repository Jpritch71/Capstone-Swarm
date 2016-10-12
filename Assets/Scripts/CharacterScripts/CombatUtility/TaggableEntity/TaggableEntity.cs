using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class TaggableEntity : Initializer, I_Entity
{
    public bool Killable { get; protected set; }
    public bool Dead { get; protected set; }
    public float BaseIntegrity { get; private set; }
    public float Integrity { get; private set; }

    private List<I_EntityTag> tags;
    private List<I_EntityTag> Tags
    {
        get
        {
            if (tags == null)
            {
                tags = new List<I_EntityTag>();
            }
            return tags;
        }
    }

    public TaggableEntity()
    {
        Killable = true;
    }

    public Vector3 Pos { get; protected set; }
    public float groundPosY { get; protected set; }

    public float initIntegrity
    {
        set
        {
            BaseIntegrity = value;
            Integrity = BaseIntegrity;
        }
    }

    public void RevokeImmunity()
    {
        //Debug.Log("Revoke");
        Killable = true;
    }

    public void IncurDamage(float damageIn)
    {
        if (!Killable)
        {
            print("not killable");
            return;
        }
        //print("damage incured: " + damageIn + " Integrity remaining: " + Integrity);

        Integrity -= damageIn;
        if (Integrity <= 0)
        {
            DeathAction();
        }
    }

    public virtual void DeathAction()
    {   
        foreach (I_EntityTag t in Tags)
        {
            t.TagAction();
        }
        DeathWork();
    }
    protected abstract void DeathWork();

    public void TagEntity(I_EntityTag tagIn)
    {
        Tags.Add(tagIn);
    }

    #region Components
    public GameObject _AttachedGameObject
    {
        get
        {
            return this.gameObject;
        }
    }

    public abstract I_Movement _MovementComponent
    {
        get;
    }
    #endregion
}