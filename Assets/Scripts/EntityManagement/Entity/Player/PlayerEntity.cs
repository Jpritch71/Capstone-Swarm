using UnityEngine;
using System.Collections;
using System;
using Combat;

public class PlayerEntity : TaggableEntity
{    
    public static int PlayerID;
    public GameObject C_AttatchedGameObject { get; protected set; }
    public override int Unique_ID { get { return C_AttatchedGameObject.GetInstanceID(); } }

    public PlayerEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn)
    {
        WeaponManager = new WeaponContainer();
        Owner_C_Controller = controllerMonoBehavior;
        C_AttatchedGameObject = GameObject.Find("PlayerUnit");
        C_AttatchedGameObject.AddComponent<Entity_MonoBehaviour>();

        I_Entity en = this;
        C_MonoBehavior = C_AttatchedGameObject.GetComponent<Entity_MonoBehaviour>();
        C_MonoBehavior.SetOwner(ref en);
        C_Collider = C_MonoBehavior.GetComponent<Collider>();
        int ID = EntityManager.AddEntity_GenerateID(ref en);

        EntityLevel = levelIn;
        DefenseModifiers = new CombatModifierHandler();

        Killable = true;
        PlayerID = Unique_ID;

        InitializeIntegrity(baseIntegrityIn);
    }

    public override string Name
    {
        get
        {
            return PlayerStats.PlayerStatContainer._PlayerInfo.UserName;
        }
    }

    protected override void DeathWork()
    {
        C_MonoBehavior._MSG("Player Dead");
        GameObject.Find("warrior2swords").GetComponent<Renderer>().enabled = false;
        GameManager._Instance.GameOver();
    }

    public override void UpdateAction()
    {
        AddIntegrity(1f);
    }

    public override void IncurDamage(float damageIn)
    {
        base.IncurDamage(damageIn);
        SARSiteUI._Instance.s_health = "|Health|\n" + string.Format("{0:0.0%}", (Integrity / BaseIntegrity));
    }
}
