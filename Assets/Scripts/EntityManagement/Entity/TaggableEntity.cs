using UnityEngine;
using System.Collections.Generic;
using System;
using Combat;

public abstract class TaggableEntity : Entity
{
    private List<I_EntityTag> tags;
    private List<I_EntityTag> Tags
    {
        get
        {           
            return tags;
        }
    }

    public TaggableEntity() : base()
    {
        tags = new List<I_EntityTag>();
    }

    public TaggableEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {
        tags = new List<I_EntityTag>();
    }    

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
            C_MonoBehavior._MSG("notKillable");
            return;
        }
        C_MonoBehavior._MSG("damage incured: " + damageIn + " Integrity remaining: " + Integrity);

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
    public int Unique_ID { get { return Owner_C_Controller.C_AttachedGameObject.GetInstanceID(); } }

    public WeaponContainer AttackManager { get; protected set; }

    public Vector3 Pos
    {
        get
        {
            return Owner_C_Controller.C_Movement.Pos;
        }
    }

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

    public abstract string Name { get; }
    public int EntityLevel { get; protected set; }
    public CombatModifierHandler DefenseModifiers { get; protected set; }

    public Entity()
    {
        AttackManager = new WeaponContainer();
        DefenseModifiers = new CombatModifierHandler();
    }

    public Entity(I_Controller ownerIn)
    {
        AttackManager = new WeaponContainer();
        Owner_C_Controller = ownerIn;
        Owner_C_Controller.C_AttachedGameObject.AddComponent<Entity_MonoBehaviour>();

        I_Entity en = this;
        C_MonoBehavior = Owner_C_Controller.C_AttachedGameObject.GetComponent<Entity_MonoBehaviour>();
        C_MonoBehavior.SetOwner(ref en);
        C_Collider = C_MonoBehavior.GetComponent<Collider>();
        int ID = EntityManager.AddEntity_GenerateID(ref en);

        EntityLevel = 1;
        DefenseModifiers = new CombatModifierHandler();

        Killable = false;
    }

    public Entity(I_Controller ownerIn, float baseIntegrityIn, int levelIn)
    {
        Owner_C_Controller = ownerIn;
        Owner_C_Controller.C_AttachedGameObject.AddComponent<Entity_MonoBehaviour>();

        I_Entity en = this;
        C_MonoBehavior = Owner_C_Controller.C_AttachedGameObject.GetComponent<Entity_MonoBehaviour>();
        C_MonoBehavior.SetOwner(ref en);
        int ID = EntityManager.AddEntity_GenerateID(ref en);

        EntityLevel = levelIn;
        DefenseModifiers = new CombatModifierHandler();

        Killable = false;
        InitializeIntegrity(baseIntegrityIn);
        AttackManager = new WeaponContainer();
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

    public abstract void IncurDamage(float damageIn);
    public abstract void DeathAction();

    public I_Controller Owner_C_Controller { get; protected set; }

    public Collider C_Collider { get; protected set; }

    public Entity_MonoBehaviour C_MonoBehavior { get; protected set; }    
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