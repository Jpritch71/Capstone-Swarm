using UnityEngine;
using System.Collections.Generic;

public abstract class TaggableEntity : MonoBehaviour, I_Entity
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

    protected abstract void InitAwake();
    protected abstract void InitStart();

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
        //print("deaths, tag count: " + Tags.Count);
        foreach (I_EntityTag t in Tags)
        {
            t.TagAction();
        }
    }

    public void TagEntity(I_EntityTag tagIn)
    {
        Tags.Add(tagIn);
    }

    public GameObject _AttachedGameObject
    {
        get
        {
            return this.gameObject;
        }
    }
}