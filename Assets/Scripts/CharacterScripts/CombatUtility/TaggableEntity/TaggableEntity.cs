using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class TaggableEntity : Entity
{
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

    public override void IncurDamage(float damageIn)
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

    public override void DeathAction()
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
}


public abstract class Entity : Initializer, I_Entity
{
    public int Unique_ID { get; private set; }

    public bool Killable { get; protected set; }
    public bool Dead { get; protected set; }
    public float BaseIntegrity { get; protected set; }
    public float Integrity { get; protected set; }

    public Entity()
    {
        I_Entity en = this;
        Unique_ID = EntityManager.GetUniqueID(ref en);
    }

    public abstract void IncurDamage(float damageIn);
    public abstract void DeathAction();

    public abstract GameObject _AttachedGameObject { get; }
    public abstract I_Movement _MovementComponent { get; }    
}