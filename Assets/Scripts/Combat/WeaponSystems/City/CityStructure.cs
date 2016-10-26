using UnityEngine;
using System.Collections;

public class PlayerStructure : TaggableEntity
{
    public override int EntityLayer { get { return 11; } }
    public bool StructureAlive { get; protected set; }

    public override string Name
    {
        get
        {
            return Owner_C_Controller.C_AttachedGameObject.name;
        }
    }

    private GameObject fire;

    public GameObject targetPointContainer;

    public PlayerStructure(I_Controller controllerMonoBehavior, float baseIntegrityIn, int levelIn) : base(controllerMonoBehavior, baseIntegrityIn, levelIn)
    {
        StructureAlive = true;

        fire = C_MonoBehavior.transform.Find("Fire").gameObject;
        fire.SetActive(false);
    }

    // Use this for initialization
    //   void Start ()
    //   {
    //       CityAlive = true;
    //       fire = transform.Find("Fire").gameObject;
    //       fire.SetActive(false);

    //       foreach(Transform t in targetPointContainer.transform)
    //       {
    //           t.gameObject.AddComponent<TargetPoint>();
    //           t.gameObject.GetComponent<TargetPoint>().parentCity = this;
    //       }
    //}

    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    if(col.collider.gameObject.layer == 8)
    //    {
    //        IncurDamage(500f);
    //    }
    //}

    protected override void DeathWork()
    {
        StructureAlive = false;
        fire.SetActive(true);
        ObeliskManager.CheckStatus();
    }

    public Transform GetRandomTargetPoint()
    {
        return targetPointContainer.transform.GetChild(Random.Range(0, targetPointContainer.transform.childCount));
    }
}
