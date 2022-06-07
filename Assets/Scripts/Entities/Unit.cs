using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Unit : BaseEntity
{
    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;
    public BaseEntity EntityTarget = null;
    public TargetBuilding CaptureTarget = null;
    public NavMeshAgent NavMeshAgent;
    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }

    public bool isCapturing = false;
    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }
    void Unit_OnDead()
    {
        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        Destroy(gameObject);
    }

	/* Events */
	public Action OnChangeSquadEvent;

    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {

	}
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos)
    {
        if (EntityTarget != null)
            EntityTarget = null;

        if (CaptureTarget != null)
            StopCapture();

        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (CanAttack(target) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (target.GetTeam() != GetTeam())
            StartAttacking(target);
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (EntityTarget != null)
            EntityTarget = null;

        if (IsCapturing())
            StopCapture();

        if (target.GetTeam() != GetTeam())
            StartCapture(target);
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity)
    {
        if (CanRepair(entity) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (entity.GetTeam() == GetTeam())
            StartRepairing(entity);
    }

	private class DistComparer : IComparer<Collider>
	{
		public Vector3 pos;

		public DistComparer(Vector3 pos_) => pos = pos_;

		public int Compare(Collider x, Collider y)
		{
			if (x == y)
				return 0;

			float distX = (x.transform.position - pos).sqrMagnitude;
			float distY = (y.transform.position - pos).sqrMagnitude;

			if (distX < distY)
				return 1;
			if (distX > distY)
				return -1;

			return 0;
		}
	}

	public bool CanSeeEnemy()
    {
        Collider[] targetColliders = Physics.OverlapSphere(transform.position, GetUnitData.AttackDistanceMax);

		/* sort from closest to farthest */
		DistComparer distComparer = new DistComparer(transform.position);
		targetColliders = targetColliders.OrderBy(e => e, distComparer).ToArray();

        foreach (var targetCollider in targetColliders)
        {
            BaseEntity enemy = targetCollider.GetComponent<BaseEntity>();
            if (enemy != null && enemy.GetTeam() != this.GetTeam())
            {
                EntityTarget = enemy;
                return true;
            }
        }
        return false;
    }

    public bool CanSeeAlly()
    {
        Collider[] targetColliders = Physics.OverlapSphere(transform.position, GetUnitData.AttackDistanceMax);

        DistComparer distComparer = new DistComparer(transform.position);
        targetColliders = targetColliders.OrderBy(e => e, distComparer).ToArray();

        foreach (var targetCollider in targetColliders)
        {
            Unit ally = targetCollider.GetComponent<Unit>();
            if (ally != null && ally.GetTeam() == this.GetTeam() && ally.HP < ally.UnitData.MaxHP)
            {
                EntityTarget = ally;
                return true;
            }
        }
        return false;
    }

    public bool CanSeePoint()
    {
        Collider[] targetColliders = Physics.OverlapSphere(transform.position, GetUnitData.CaptureDistanceMax);
        foreach (var targetCollider in targetColliders)
        {
            TargetBuilding point = targetCollider.GetComponent<TargetBuilding>();
            if (point != null)
            {
               CaptureTarget = point;
                return true;
            }
        }
        return false;
    }

    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
		target.OnDeadEvent += () => { EntityTarget = null; };
    }
    public void ComputeAttack()
    {
        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(EntityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            EntityTarget.AddDamage(damages);
        }
    }
    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }

    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        CaptureTarget = target;
        CaptureTarget.StartCapture(this);
        isCapturing = true;
    }
    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;
    }

    public bool IsCapturing()
    {
        return CaptureTarget != null;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        if (target.NeedHeal == false)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            EntityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;
        Unit ally = EntityTarget.GetComponent<Unit>();

        if (ally.HP < ally.UnitData.MaxHP)
        {
            if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
            {
                LastActionDate = Time.time;

                // apply reparing
                int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);

                if ((ally.HP += amount) <= ally.UnitData.MaxHP)
                {
                    EntityTarget.Repair(amount);
                }
                else
                {
                    ally.HP = ally.UnitData.MaxHP;
                    ally.NeedHeal = false;
                }
            }
        }
        
    }
    #endregion
}
