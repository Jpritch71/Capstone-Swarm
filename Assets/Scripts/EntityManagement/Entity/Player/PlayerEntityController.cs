using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PlayerEntityController : GroupUnitController
{
    public override int EntityLayer { get { return (int)Flags.Player; } }
    public Transform weapon;

    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    public TaggableEntity TargetAsTaggable { get; protected set; }
    public GroupUnitMovement TargetAs_Unit { get; protected set; }
    public override I_Entity Target
    {
        get
        {
            return TargetAsTaggable;
        }
    }

    public PlayerTargetTag targetTag;
    public Transform targetVisualizer;

    protected AttackButtonHandler buttonHandler;

    public GameObject roundModel;

    protected float clickThreshold = .15f;
    protected bool clickHeld = false;
    protected Timer clickTimer;

    // Use this for initialization
    protected override void InitAwake()
    {
        C_GroupManager = transform.parent.parent.GetComponent<PlayerGroupManager>();
        C_Movement = GetComponent<GroupUnitMovement>();
        C_Movement.C_Controller = this;       
        AnimController = new PlayerAnimController(GameObject.Find("PlayerUnit").GetComponentInChildren<Animation>());
        C_StateMachine = new StateMachine(null, new S_Player_Idle(this));

        C_SquadController = C_AttachedGameObject.transform.parent.parent.GetComponent<SquadController>();        
    }

    protected override void InitStart()
    {
        LoadStats();
    }

    public override void LoadStats()
    {
        PlayerEntityController _this = this;
        I_Entity en = null;
        if (!PlayerStats.TryLoadPlayer(ref _this, ref en))
        {
            C_Entity = new PlayerEntity(this, 1000f, 1);
        }
        else
        {
            C_Entity = en;
        }
        C_PlayerGridMovement.BaseSpeed = 10f + (C_Entity.EntityLevel / 10);
        MeleeWeapon PlayerSwordRight =
                                new MeleeWeapon(C_Entity.WeaponManager, C_Entity.C_MonoBehavior, weapon,
                                                    Combat.WeaponType.Axe, 3.5f, "Espada", 0, 100f);

        C_Entity.WeaponManager.AddWeapon(PlayerSwordRight.Unique_ID, PlayerSwordRight);
        PlayerSwordRight.AddAttack(0, new MeleeAttack(C_Entity, PlayerSwordRight, .5f, "Sword Strike"));

        WeaponStats wep;
        AmmoType round;
        AmmoBay ammo;
        WeaponManager.AmmoTag ammoTag;

        ammoTag = WeaponManager.AmmoTag.Bullet;
        round.ammoModel = roundModel;// GameObject.Find("PlayerControlledGun").GetComponent<PlayerController>().GetProjectileModel(ammoTag);
        round.projectileType = "basic_projectile";
        ammo = new AmmoBay(round, ammoTag, new object[] { 100f, 115f });
        wep = new WeaponStats(100, 1, 1f, 1f, 15f, .051f, true, 1.5f, ammo);

        RangedWeapon Player_FAKE_Cannon =
                                new RangedWeapon(C_Entity.WeaponManager, wep, C_Entity.C_MonoBehavior, weapon,
                                                    Combat.WeaponType.Firearm,
                                                        1f, "Espada_Fake_Cannon", 0, 500f);

        C_Entity.WeaponManager.AddWeapon(Player_FAKE_Cannon.Unique_ID, Player_FAKE_Cannon);
        Player_FAKE_Cannon.AddAttack(0, new CannonAttack(C_Entity, Player_FAKE_Cannon, .5f, "Cannon Attack"));

        buttonHandler = GameObject.Find("ButtonBar").GetComponent<AttackButtonHandler>();

        buttonHandler.SetButton(0, PlayerSwordRight, 0);
        buttonHandler.SetButton(1, Player_FAKE_Cannon, 0);

        C_Player_AsTaggable = (TaggableEntity)C_Entity;
        PlayerStats.PlayerStatContainer._PLAYER = (PlayerEntity)C_Entity;
    }

    // Update is called once per frame
    private bool IgnoreClicks = false;
    void Update()
    {
        if (C_StateMachine != null)
            C_StateMachine.ExecuteUpdate(); //update State Machine
        UpdateTargetVisualizer();

        IgnoreClicks = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (!IgnoreClicks)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(clickTimer == null)
                {
                    clickTimer = Timer.Register(clickThreshold, () =>
                                                                {
                                                                    clickHeld = true;
                                                                }, null, false, true, this);
                }

                if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, out hit, Mathf.Infinity, (int)Flags.Enemy, QueryTriggerInteraction.Ignore))
                {
                    print(hit);
                    I_Entity en = null;
                    if (EntityManager.GetEntityByCollider(hit.collider, ref en))
                    {
                        //print(en);
                        SetTarget((TaggableEntity)en);
                        return;
                    }
                }
            }

            if (clickHeld)
            {
                if (Input.GetMouseButton(0) && C_PlayerGridMovement.CanMove)
                {
                    if (!C_PlayerGridMovement.DirectionalMovement)
                    {
                        C_PlayerGridMovement.StopMoving();
                    }
                    C_PlayerGridMovement.StartFreeMoving();
                    ClearTargetVisualizer();
                }
            }

            if (Input.GetMouseButtonDown(1) && !C_PlayerGridMovement.DirectionalMovement)
            {
                hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, Mathf.Infinity, (1 << 8) + (1 << 9));

                if (hits.Length > 0)
                {
                    hit = hits[0];
                    for (int x = 1; x < hits.Length; x++)
                    {
                        if (hit.point.y < hits[x].point.y)
                            hit = hits[x];
                    }
                    //marker.transform.position = hit.point;
                    C_PlayerGridMovement.SetPathToPoint(hit.point);
                    ClearTargetVisualizer();
                }
            }
        }
        if (Input.GetMouseButtonUp(0) || !C_PlayerGridMovement.CanMove)
        {
            C_PlayerGridMovement.EndFreeMove();
            if(clickTimer != null)
            {
                clickTimer = null;
                clickHeld = false;
            }
        }

        if (TargetAsTaggable != null)
        {
            if (Vector3.Distance(C_Entity.Pos, TargetAsTaggable.Pos) > C_Entity.WeaponManager.ActiveWeapon.WeaponRange)
                C_SquadController.MoveToEntity(TargetAsTaggable);
            else
            {
                TryClickButton(1);
            }
        }

        if (AttackKeyPressed(1))
        {
            
        }
        if (AttackKeyPressed(2))
        {
        }

        if(clickTimer != null && clickTimer.isCompleted)
        {
            clickTimer = null;
        }
    }

    protected bool AttackKeyPressed(int alphaValue)
    {
        if (Input.GetKeyDown(alphaValue + KeyCode.Alpha0))
        {
            return TryClickButton(alphaValue);
        }
        return false;
    }

    protected bool TryClickButton(int keyIndex)
    {
        return buttonHandler.ClickButton(keyIndex);
    }

    private void UpdateTargetVisualizer()
    {
        if(TargetAsTaggable != null)
        {
            targetVisualizer.position = TargetAsTaggable.Pos + (Vector3.up * 1); //Target.C_Collider.bounds.extents.y);
            if (!C_Movement.Moving)
            {
                C_GroupManager.LeaderUnit.MovementOrders = (TargetAsTaggable.Pos - C_Entity.Pos);
                C_Movement.SetFacingDirection();
                C_GroupManager.LeaderUnit.MovementOrders = Vector3.zero;
            }
        }
    }

    private void SetTarget(TaggableEntity entityIn)
    {
        if (TargetAsTaggable != null)
        {
            TargetAsTaggable.RemoveTag(targetTag);
        }

        TargetAsTaggable = entityIn;
        TargetAs_Unit = TargetAsTaggable.Owner_C_Controller.C_Movement;

        targetTag = new PlayerTargetTag(this, TargetAs_Unit);
        TargetAsTaggable.TagEntity(targetTag);
    }

    public void ClearTargetVisualizer()
    {
        TargetAsTaggable = null;
        if(TargetAs_Unit != null)
        {
            GroupUnitMovement nextUnit = null;
            foreach(GroupUnitMovement unit in TargetAs_Unit.Squad.C_Squad_HashSet)
            {
                if(unit != null)
                    if(unit != TargetAs_Unit)
                    {
                        nextUnit = unit;
                        break;
                    }
            }
            if (nextUnit != null)
            {
                SetTarget((TaggableEntity)nextUnit.C_Controller.C_Entity);
            }   
            else
            {
                TargetAs_Unit = null;
            }   
        }
        
        targetVisualizer.transform.position = new Vector3(-999, -999, -999);
    }

    #region Components
    public PlayerMovement C_PlayerGridMovement
    {
        get
        {
            return (PlayerMovement)(this.C_GroupManager.GroupGridComponent);
        }
    }
    public PlayerAnimController AnimController { get; protected set; }
    public PlayerGroupManager C_GroupManager { get; protected set; }

    public TaggableEntity C_Player_AsTaggable { get; protected set; }
    #endregion
}

public class PlayerTargetTag : I_EntityTag
{
    private PlayerEntityController player;
    private GroupUnitMovement tagged_unit;

    public PlayerTargetTag(PlayerEntityController player, GroupUnitMovement unitIn)
    {
        this.player = player;
        tagged_unit = unitIn;
    }

    public void TagAction()
    {
        player.ClearTargetVisualizer();
    }
}
