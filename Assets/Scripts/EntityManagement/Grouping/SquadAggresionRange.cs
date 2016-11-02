using System.Collections.Generic;
using UnityEngine;

public class SquadAggresionRange : Initializer, I_Sight
{
    [SerializeField]
    private TaggableEntity _Target;

    public TaggableEntity Target { get { return _Target; } protected set { _Target = value; } }
    protected Vector3 targetPos { get { return Target.Pos; } }
    protected Vector3 ownerPos { get { return C_OwnerGroup.GroupGridComponent.Pos; } }

    protected Queue<TaggableEntity> TargetQueue;
    protected HashSet<TaggableEntity> Targets;

    protected int visionTargetFlag;

    protected override void InitAwake()
    {
        VisionCollider = GetComponent<SphereCollider>();
        C_OwnerGroup = transform.parent.GetComponent<SquadController>();
        TargetQueue = new Queue<TaggableEntity>();
        Targets = new HashSet<TaggableEntity>();
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
        if (DistanceToTarget() > AggressionRange * 1.15f)
        {
            ClearTarget();
        }
    }

    protected float aggressionRange;
    public float AggressionRange
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
        get { _TargetAcquired = (Target != null);  return Target != null; }
    }

    public float DistanceToTarget()
    {
        return Vector3.Distance(targetPos, ownerPos);
    }

    /// <summary>
    /// Add an Entity to the list of targets.
    /// </summary>
    /// <param name="entityIn">Entity to add to the list.</param>
    /// <returns>Returns false if the Target already Exists in the list.</returns>
    protected bool TryAddTargetToQueue(TaggableEntity entityIn)
    {
        if (!Targets.Contains(entityIn))
        {
            entityIn.TagEntity(new VisionTag(this, entityIn));
            if (Targets.Count == 0)
            {
                Target = entityIn;
            }
            else
            {
                Targets.Add(entityIn);
                TargetQueue.Enqueue(entityIn);
            }
            return true;
        }
        return false;
    }

    protected bool TrySetTarget(TaggableEntity entityIn)
    {
        if (entityIn != null)
        {
            if (entityIn.Owner_C_Controller.C_AttachedGameObject.layer != C_OwnerGroup.C_AttachedGameObject.layer)
            {
                return TryAddTargetToQueue(entityIn);
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
        TaggableEntity previousTarget = null;
        if(Target != null)
        {
            previousTarget = Target;
        }
        entityIn.TagEntity(new VisionTag(this, entityIn));
        Target = entityIn;

        if (previousTarget != null)
            TryAddTargetToQueue(previousTarget);
    }

    protected TaggableEntity Dequeue()
    {
        TaggableEntity entity = TargetQueue.Dequeue();
        Targets.Remove(entity);
        return entity;
    }

    public void TargetOutOfRange()
    {
        TrySetTarget(null);
        if (TargetQueue.Count > 0)
        {
            Target = Dequeue();
        }
    }

    public void ClearTarget()
    {
        if (C_OwnerGroup.HuntOrder)
        {
            if (Target == ((Order_SquadAttack)(C_OwnerGroup.CurrentOrder)).TargetEntity)
            {
                ForceTarget((TaggableEntity)en);
            }
        }
        TrySetTarget(null);
        if(TargetQueue.Count > 0)
        {
            Target = Dequeue();
        }   
    }

    public bool RemoveTarget(TaggableEntity entityToRemoveIn)
    {
        if (Target == entityToRemoveIn)
            ClearTarget();
        if(Targets.Contains(entityToRemoveIn))
        {
            var list = new List<TaggableEntity>(TargetQueue);
            list.Remove(entityToRemoveIn);
            Targets.Remove(entityToRemoveIn);
            TargetQueue = new Queue<TaggableEntity>(list);

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
            if(C_OwnerGroup.HuntOrder)
            {
                if (en == ((Order_SquadAttack)(C_OwnerGroup.CurrentOrder)).TargetEntity)
                {
                    ForceTarget((TaggableEntity)en);
                }
            }            
            else
                TrySetTarget((TaggableEntity)en);
        }
    }

    protected SquadController C_OwnerGroup { get; set; }
}

public class VisionTag : I_EntityTag
{
    private SquadAggresionRange visionObject;
    private TaggableEntity taggedEntity;
    public VisionTag(SquadAggresionRange visionObjectIn, TaggableEntity entityIn)
    {
        visionObject = visionObjectIn;
        taggedEntity = entityIn;
    }

    public void TagAction()
    {
        visionObject.RemoveTarget(taggedEntity);
    }
}

/// <summary>
/// Object to handle position tracking for moving entities.
/// If an entity moves in between updates, the signal is flipped to indicate a change in position.
/// </summary>
public class MovementSignaler
{
    public I_Entity TrackedEntity { get; protected set; }
    public bool MovementSignal { get; protected set; }

    /// <summary>
    /// Timer that fires after checkTime Seconds. Calls CheckTracks,.
    /// </summary>
    protected Timer updateTimer;
    /// <summary>
    /// How often we should check to see if the target has moved.
    /// </summary>
    protected float checkTime;

    /// <summary>
    /// Last known position of the target.
    /// </summary>
    protected Vector3 lastPos;

    public MovementSignaler(I_Entity entityIn, float checkTimeIn)
    {
        TrackedEntity = entityIn;
        MovementSignal = false;
        lastPos = TrackedEntity.Pos;
        checkTime = checkTimeIn;

        StartTimer();
    }

    public void AcknowledgeSignal()
    {
        MovementSignal = false;
    }

    /// <summary>
    /// Compares the targets last known position with its current position. If a change is detected, the movement signal is flipped.
    /// </summary>
    protected void CheckTracks()
    {
        if(TrackedEntity == null)
        {
            KillSignaler();
            return;
        }
        if(lastPos != TrackedEntity.Pos)
        {
            MovementSignal = true;
            lastPos = TrackedEntity.Pos;
        }
        StartTimer();
    }

    protected void StartTimer()
    {
        updateTimer = Timer.Register(checkTime, CheckTracks, null, false, false, null);
    }

    public void KillSignaler()
    {
        updateTimer.Cancel();
        updateTimer = null;
        TrackedEntity = null;
    }
}
