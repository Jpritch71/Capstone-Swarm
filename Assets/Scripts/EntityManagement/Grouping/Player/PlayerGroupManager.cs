using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGroupManager : SquadController
{
    public override int GroupLayer { get { return (1 << 11); } }
    protected GroupUnitMovement playerUnit { get { return LeaderUnit; } set { LeaderUnit = value; } }

    protected override void InitAwake()
    {
        base.InitAwake();
        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<SquadAggresionRange>();
        C_AggresionSphere = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<SquadAggresionRange>();
    }

    protected override void InitStart()
    {
        squadObject.name = name + ": " + squadObject.name; ///INSERT PLAYER NAME HERE
        squadObject.parent = GameObject.Find("EntityHierarchy").transform;
        playerUnit = squadObject.gameObject.transform.Find("PlayerUnit").GetComponent<GroupUnitMovement>();
        AddSquadMember(playerUnit);
        lastPos = transform.position;

        gridedWeight = 5;

        SetVolumeRadius(1.5f);
        volumeCollider.radius = 1.5f;

        playerUnit.SquadSendInit();

        C_AttachedGameObject.transform.Find("VisionObject").gameObject.AddComponent<SquadAggresionRange>();
        C_AggresionSphere = C_AttachedGameObject.transform.Find("VisionObject").gameObject.GetComponent<SquadAggresionRange>();
    }

    void Update()
    {
        UpdateGroup();
    }   

    protected override void UpdateGroup()
    {
        if (GroupDead)
            return;
        deltaMovement = transform.position - lastPos;

        playerUnit.MovementOrders += deltaMovement;
        lastPos = transform.position;

        CenterOfMass = Pos;
    }

    protected override void GroupLogic()
    {

    }

    protected override void SetVolumeRadius(float radiusIn)
    {
        currentVolumeRadius = radiusIn;
        if (C_Squad_HashSet == null)
            return;
        foreach (GroupUnitMovement g in C_Squad_HashSet)
        {
            g.DistanceThreshold = radiusIn * 2;
        }
    }
}
