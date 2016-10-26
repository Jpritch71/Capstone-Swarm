using UnityEngine;
using System.Collections;
using System;

public abstract class SquadController : Initializer, I_Controller
{
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    // Use this for initialization
    protected override void InitAwake()
    {
        C_Movement = GetComponent<A_GridMover>();
        C_GroupManager = GetComponent<GroupManager>();
    }

    protected override void InitStart()
    {
        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<SquadVision>();
        C_Vision = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<SquadVision>();
        LoadStats();
    }

    public abstract void LoadStats();

    #region Components
    public I_Movement C_Movement
    {
        get
        {
            return C_GridMovement;
        }
        protected set
        {
            C_GridMovement = (A_GridMover)value;
        }
    }
    public A_GridMover C_GridMovement { get; private set; }

    public GroupManager C_GroupManager { get; protected set; }

    public I_Entity C_Entity
    {
        get; protected set;
    }

    public StateMachine C_StateMachine
    {
        get; protected set;
    }

    public I_Vision C_Vision { get; protected set; }
    #endregion
}

public class SquadVision : Initializer, I_Vision
{
    public TaggableEntity Target { get; protected set; }
    protected Vector3 targetPos { get { return Target.Pos; } }
    protected Vector3 ownerPos { get { return Owner_C_Controller.C_Movement.Pos; } }

    protected override void InitStart()
    {
        
    }

    void Update()
    {
        if (!TargetAcquired)
            return;
        Debug.DrawLine(targetPos, ownerPos, Color.red);
        if(DistanceToTarget() > VisionRange)
        {
            AddTarget(null); //clear the target
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

    public bool TargetAcquired
    {
        get { return Target != null; }
    }

    public float DistanceToTarget()
    {
        return Vector3.Distance(targetPos, ownerPos) * 1.1f;
    }

    public bool AddTarget(TaggableEntity entityIn)
    {
        if (entityIn != null)
        {
            if (entityIn.Owner_C_Controller.C_AttachedGameObject.layer == Owner_C_Controller.C_AttachedGameObject.layer)
            {
                return false;
            }
            entityIn.TagEntity(new VisionTag(this));
            Target = entityIn;
            return true;
        }
        print("nulling target");
        Target = entityIn;
        return true;
    }

    private I_Entity en = null;
    void OnTriggerEnter(Collider other)
    {
        var a = Owner_C_Controller;
        var b = Owner_C_Controller.C_Entity;
         int bitwise = ((1 << other.gameObject.layer) & ((1 << Owner_C_Controller.C_Entity.EntityLayer) | WorldManager.mapFlag | WorldManager.entityUtilityFlag));
        if (TargetAcquired || ((1 << other.gameObject.layer) & ((1 << gameObject.layer) | WorldManager.mapFlag)) != 0)
            return;
        if(EntityManager.GetEntityByCollider(other, ref en))
        {
            if(en.Unique_ID == PlayerEntity.PlayerID)
            if (AddTarget((TaggableEntity)en))
                return;
        }          
    }

    protected override void InitAwake()
    {
        VisionCollider = GetComponent<SphereCollider>();
        Owner_C_Controller = transform.parent.GetComponent<I_Controller>();
    }

    protected I_Controller Owner_C_Controller { get; set; }
}

public class VisionTag : I_EntityTag
{
    private I_Vision visionObject;
    public VisionTag(I_Vision visionObjectIn)
    {
        visionObject = visionObjectIn;
    }

    public void TagAction()
    {
        visionObject.AddTarget(null);
    }
}