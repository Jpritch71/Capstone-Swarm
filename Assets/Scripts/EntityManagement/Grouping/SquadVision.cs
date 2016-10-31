using System.Collections.Generic;
using UnityEngine;

public class SquadVision : Initializer, I_Vision
{
    [SerializeField]
    private TaggableEntity _Target;

    public TaggableEntity Target { get { return _Target; } protected set { _Target = value; } }
    protected Vector3 targetPos { get { return Target.Pos; } }
    protected Vector3 ownerPos { get { return C_OwnerGroup.GroupGridComponent.Pos; } }

    protected List<TaggableEntity> Targets;

    protected int visionTargetFlag;

    protected override void InitAwake()
    {
        VisionCollider = GetComponent<SphereCollider>();
        C_OwnerGroup = transform.parent.GetComponent<SquadController>();
        Targets = new List<TaggableEntity>();
    }

    protected override void InitStart()
    {
        visionTargetFlag = (int)(Flags.Entities) ^ (1 << C_OwnerGroup.GroupLayer);
    }

    void Update()
    {
        if (!TargetAcquired)
            return;
        Debug.DrawLine(targetPos, ownerPos, Color.red);
        if (DistanceToTarget() > VisionRange * 1.15f)
        {
            ClearTarget();
        }
    }

    protected float visionRange;
    public float VisionRange
    {
        get { return visionRange; }
        set
        {
            visionRange = value;
            if (VisionCollider == null)
                return;
            VisionCollider.radius = visionRange;
        }
    }

    public SphereCollider VisionCollider { get; protected set; }

#if UNITY_EDITOR
    [SerializeField]
    private bool _TargetAcquired;
#endif
    public bool TargetAcquired
    {
        get { _TargetAcquired = (Target != null);  return Target != null; }
    }
#if UNITY_EDITOR
    [SerializeField]
    private float _DistanceToTarget;
#endif
    public float DistanceToTarget()
    {
#if UNITY_EDITOR
        _DistanceToTarget = Vector3.Distance(targetPos, ownerPos);
#endif
        return Vector3.Distance(targetPos, ownerPos);
    }

    protected bool TrySetTarget(TaggableEntity entityIn)
    {
        if (entityIn != null)
        {
            if (entityIn.Owner_C_Controller.C_AttachedGameObject.layer != C_OwnerGroup.C_AttachedGameObject.layer)
            {
                entityIn.TagEntity(new VisionTag(this, entityIn));
                if (Target == null && Targets.Count == 0)
                {
                    Target = entityIn; 
                }
                else
                {
                    Targets.Add(entityIn);
                }
                return true;
            }
            return false;
        }
        Target = null;
        return true;
    }

    protected void ForceTarget(TaggableEntity entityIn)
    {
        if (entityIn.Owner_C_Controller.C_AttachedGameObject.layer == C_OwnerGroup.C_AttachedGameObject.layer)
        {
            return;
        }
        if(TargetAcquired)
        {
            Targets.Add(Target);
        }
        entityIn.TagEntity(new VisionTag(this, entityIn));
        Target = entityIn;
    }

    public void ClearTarget()
    {
        TrySetTarget(null);
        if(Targets.Count > 0)
        {
            Target = Targets[0];
        }
        C_OwnerGroup.ClearTarget();
    }

    public bool RemoveTarget(TaggableEntity entityToRemoveIn)
    {
        if (Target == entityToRemoveIn)
            ClearTarget();
        if(Targets.Contains(entityToRemoveIn))
        {
            Targets.Remove(entityToRemoveIn);
            return true;
        }
        return false;
    }

    private I_Entity en = null;
    void OnTriggerEnter(Collider other)
    {
        int bitwise = (1 << other.gameObject.layer) & (visionTargetFlag);
        if (bitwise == 0)
            return;

        if (EntityManager.GetEntityByCollider(other, ref en)) //if we find an entity paired with this collider
        {
            if (TrySetTarget((TaggableEntity)en))
                return;
        }
    }

    protected SquadController C_OwnerGroup { get; set; }
}

public class VisionTag : I_EntityTag
{
    private SquadVision visionObject;
    private TaggableEntity taggedEntity;
    public VisionTag(SquadVision visionObjectIn, TaggableEntity entityIn)
    {
        visionObject = visionObjectIn;
        taggedEntity = entityIn;
    }

    public void TagAction()
    {
        visionObject.RemoveTarget(taggedEntity);
    }
}