using UnityEngine;
using System.Collections;

public class ObjectiveStructure : PlayerStructure
{
    public GameObject C_AttatchedGameObject { get; protected set; }
    public static int _OBJECTIVE_ENTITY_ID { get; protected set; }

    public ObjectiveStructure(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {       
        C_AttatchedGameObject = GameObject.Find("Monument_OBJECTIVE");
        C_AttatchedGameObject.AddComponent<Entity_MonoBehaviour>();

        _OBJECTIVE_ENTITY_ID = Unique_ID;
    }

    protected override void DeathWork()
    {
        base.DeathWork();
        GameManager._Instance.GameOver();
    }

    public override void UpdateAction()
    {
        AddIntegrity(1f);
    }

    public override void IncurDamage(float damageIn)
    {
        base.IncurDamage(damageIn);
        SARSiteUI._Instance.s_objectiveHealth = "|Objective Health|\n" + string.Format("{0:0.0%}", (Integrity / BaseIntegrity));
    }
}
