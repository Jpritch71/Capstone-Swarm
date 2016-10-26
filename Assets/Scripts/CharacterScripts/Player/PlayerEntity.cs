using UnityEngine;
using System.Collections;
using System;
using Combat;

public class PlayerEntity : TaggableEntity
{
    public override int EntityLayer { get { return 11; } }
    public static int PlayerID;
    public GameObject C_AttatchedGameObject { get; protected set; }
    public new int Unique_ID { get { return C_AttatchedGameObject.GetInstanceID(); } }

    public PlayerEntity(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn)
    {
        AttackManager = new AttackContainer();
        Owner_C_Controller = controllerMonoBehavior;
        C_AttatchedGameObject = GameObject.Find("PlayerUnit");
        C_AttatchedGameObject.AddComponent<Entity_MonoBehaviour>();

        I_Entity en = this;
        C_MonoBehavior = C_AttatchedGameObject.GetComponent<Entity_MonoBehaviour>();
        C_MonoBehavior.SetOwner(ref en);
        C_Collider = C_MonoBehavior.GetComponent<Collider>();
        int ID = EntityManager.AddEntity_GenerateID(ref en);

        EntityLevel = 1;
        DefenseModifiers = new CombatModifierHandler();

        Killable = true;
        PlayerID = Unique_ID;
    }

    public override string Name
    {
        get
        {
            C_MonoBehavior._MSG("GET THE PLAYER USERNAME");
            throw new NotImplementedException();
        }
    }

    protected override void DeathWork()
    {
        C_MonoBehavior._MSG("Player Dead");
    }
}
