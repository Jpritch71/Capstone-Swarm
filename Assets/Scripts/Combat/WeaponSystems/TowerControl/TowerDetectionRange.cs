using System.Collections.Generic;
using UnityEngine;

public class TowerDetectionSphere : Initializer, I_Sight
{
    [SerializeField]
    private TaggableEntity _Target;

    public TaggableEntity Target { get { return _Target; } protected set { _Target = value; } }
    protected Vector3 targetPos { get { return Target.Pos; } }
    protected Vector3 ownerPos { get { return C_OwnerStructure.Pos; } }

    protected int visionTargetFlag;

    protected override void InitAwake()
    {
        VisionCollider = GetComponent<SphereCollider>();
        C_OwnerTower = transform.parent.GetComponent<TowerController>();
        C_OwnerStructure = (PlayerStructure)C_OwnerTower.C_Entity;
    }

    protected override void InitStart()
    {
        visionTargetFlag = (int)(Flags.Entities) ^ (C_OwnerTower.EntityLayer);
    }

    void Update()
    {
        if (!TargetAcquired)
            return;
        Debug.DrawLine(targetPos, ownerPos, Color.red);
        if (DistanceToTarget() > TrackingRange * 1.15f)
        {
            ClearTarget();
        }
    }

    protected float aggressionRange;
    public float TrackingRange
    {
        get { return aggressionRange; }
        set
        {
            aggressionRange = value;
            if (VisionCollider == null)
                return;
            VisionCollider.radius = aggressionRange;
        }
    }

    public SphereCollider VisionCollider { get; protected set; }

    [SerializeField]
    private bool _TargetAcquired;
    public bool TargetAcquired
    {
        get { _TargetAcquired = (Target != null); return Target != null; }
    }

    public float DistanceToTarget()
    {
        return Vector3.Distance(targetPos, ownerPos);
    }

    public void ClearTarget()
    {
        Target = null;
    }

    private I_Entity en = null;
    void OnTriggerEnter(Collider other)
    {
        int bitwise = (1 << other.gameObject.layer) & (visionTargetFlag);
        if (bitwise == 0)
            return;

        if (EntityManager.GetEntityByCollider(other, ref en)) //if we find an entity paired with this collider
        {
            Target = (TaggableEntity)en;
            Target.TagEntity(new TurretTowerTag(this, Target));
            C_OwnerTower.CurrentTarget = Target.Owner_C_Controller.C_AttachedGameObject;
        }
    }

    protected TowerController C_OwnerTower { get; set; }
    protected PlayerStructure C_OwnerStructure { get; set; }
}


public class TurretTowerTag : I_EntityTag
{
    private TowerDetectionSphere detectionObject;
    private TaggableEntity taggedEntity;
    public TurretTowerTag(TowerDetectionSphere detectionObjectIn, TaggableEntity entityIn)
    {
        detectionObject = detectionObjectIn;
        taggedEntity = entityIn;
    }

    public void TagAction()
    {
        detectionObject.ClearTarget();
    }
}
