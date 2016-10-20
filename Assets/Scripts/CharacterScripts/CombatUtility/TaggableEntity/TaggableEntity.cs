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

    public TaggableEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn) : base(controllerMonoBehavior)
    {
        Killable = true;
        InitializeIntegrity(baseIntegrityIn);
    }

    protected void InitializeIntegrity(float initialBaseIntegrity)
    {

        if (initialBaseIntegrity <= 0)
        {
            throw new System.ArgumentOutOfRangeException("Base Integrity",
                            "The Base Integrity of an Enitity must be greater than 0!\nMethod entered with attempted Base Integrity of [: " + initialBaseIntegrity + "]");
        }
        BaseIntegrity = initialBaseIntegrity;
        Integrity = BaseIntegrity;
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
        Dead = true;
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


public abstract class Entity : I_Entity
{
    public int Unique_ID { get; private set; }

    private bool killable = false;
    //public bool Initialized { get; protected set;}    
    /// <summary>
    /// At Instantiation, all entities are invulerable (killable = false)
    /// </summary>
    public bool Killable
    {
        get
        {
            return killable;
        }
        protected set
        {
            killable = value;
            //Initialized = true;
        }
    }
    public bool Dead { get; protected set; }
    public float BaseIntegrity { get; protected set; }
    public float Integrity { get; protected set; }

    public Entity(I_Controller ownerIn)
    {
        _Owner_Controller = ownerIn;
        _Owner_Controller.C_AttachedGameObject.AddComponent<Entity_MonoBehaviour>();

        I_Entity en = this;
        C_MonoBehavior = _Owner_Controller.C_AttachedGameObject.GetComponent<Entity_MonoBehaviour>();
        C_MonoBehavior.SetOwner(ref en);
        Unique_ID = EntityManager.GetUniqueID(ref en);
    }

    public abstract void IncurDamage(float damageIn);
    public abstract void DeathAction();

    public I_Controller _Owner_Controller { get; protected set; }

    protected Entity_MonoBehaviour C_MonoBehavior { get; set; }
}

public class Entity_MonoBehaviour : MonoBehaviour
{
    public I_Entity Owner_Entity { get; protected set; }
    public void SetOwner(ref I_Entity enIn)
    {
        Owner_Entity = enIn;
    }

    void Awake()
    {
       
    }

    void Start()
    {
       
    }

    public void _MSG(object o)
    {
        print(o);
    }
}