using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/* a class that represents a group. 
 * Will be used to make move and stop a group of units
 * as Boids. */
public class Squad
{
	/*===== Members =====*/

	private List<Unit> _group	= new List<Unit>();

	private Vector3	_desiredPosition	= Vector3.zero;
	private	Vector3	_currentPos			= Vector3.zero;
	private bool	_moves				= false;
	private float	_radius				= 0.0f;

	private List<float> _previousSpeeds = new List<float>();

	/*===== Accessor =====*/

	public int		Count	=> _group.Count;
	public bool		Moving	=> _moves;
	public Vector3	Position => _currentPos;
	public Vector3	DesiredPos => _desiredPosition;

	public void UpdatePosition()
	{
		for (int i = 0; i < _group.Count; i++)
		{
			_currentPos += _group[i].transform.position;
		}

		_currentPos /= _group.Count;
	}

	public int GetCost()
	{
		int cost = 0;

		for (int i = 0; i < _group.Count; i++)
		{
			cost += _group[i].GetUnitData.Cost;
		}

		return cost;
	}

	public bool Capturing()
	{
		bool temp = true;

		foreach(Unit unit in _group)
		{
			temp &= unit.CaptureTarget != null;
		}

		return temp;
	}

	public bool Attacking()
	{
		bool temp = true;

		foreach (Unit unit in _group)
		{
			temp &= unit.EntityTarget != null && unit.EntityTarget.GetTeam() != unit.GetTeam();
		}

		return temp;
	}

	public void StopMove()
	{
		foreach (Unit unit in _group)
		{
			bool isAttacking = unit.EntityTarget != null && unit.EntityTarget.GetTeam() != unit.GetTeam();

			if (isAttacking)
				unit.NavMeshAgent.isStopped = true;
			else if (unit.CaptureTarget != null)
				unit.NavMeshAgent.isStopped = false;
		}
	}

	/*====== Add/Remove =====*/

	public void Add(Unit newUnit_)
	{
		newUnit_.OnChangeSquadEvent?.Invoke();
		newUnit_.OnChangeSquadEvent += () => { _group.Remove(newUnit_); };
		newUnit_.OnDeadEvent += () => { _group.Remove(newUnit_); };
		_group.Add(newUnit_);
		ResetMoveData();
	}

	public void Remove(Unit oldUnit_)
	{
		ResetMoveData();
		_group.Remove(oldUnit_);
	}

	public void AddRangeGroup(List<Unit> newGroup_)
	{
		newGroup_.ForEach(unit => unit.OnChangeSquadEvent?.Invoke());
		ResetMoveData();
		_group.Clear();
		_group.AddRange(newGroup_);
		newGroup_.ForEach(unit => unit.OnChangeSquadEvent += () => { _group.Remove(unit); }); 
		newGroup_.ForEach(unit => unit.OnDeadEvent += () => { _group.Remove(unit); });
	}

	/*===== Init/Reset Methods =====*/

	private void InitMoveData()
	{
		/* special case: a single unit in squad => no need to have the squad computation. */
		if (_group.Count == 1)
		{
			_group[0].NavMeshAgent.SetDestination(_desiredPosition);
			_group[0].NavMeshAgent.isStopped	= false;
			_group[0].NavMeshAgent.autoBraking	= true;
			_moves = true;
			return;
		}

		/* we'll compute the mean, and the standard deviation for the squad speed */
		float overrallSpeed		= 0.0f;
		float overrallSpeedSqrd = 0.0f;
		float minSpeed			= float.MaxValue;

		for (int i = 0; i < _group.Count; i++)
		{
			/* change values of unit  */
			_group[i].NavMeshAgent.SetDestination(_desiredPosition);
			_group[i].NavMeshAgent.isStopped	= false;
			_group[i].NavMeshAgent.autoBraking	= false;

			/* squad values */
			_currentPos += _group[i].transform.position;
			_radius		+= _group[i].NavMeshAgent.radius;

			/* speed */
			overrallSpeed		+= _group[i].NavMeshAgent.speed;
			overrallSpeedSqrd	+= _group[i].NavMeshAgent.speed * _group[i].NavMeshAgent.speed;
			minSpeed			= Mathf.Min(_group[i].NavMeshAgent.speed, minSpeed);
			_previousSpeeds.Add(_group[i].NavMeshAgent.speed);
		}

		_currentPos		/= _group.Count;
		_radius			/= _group.Count;

		/* create our mean and our variance */
		overrallSpeed		/= _group.Count;
		overrallSpeedSqrd	/= _group.Count;
		overrallSpeedSqrd	-= overrallSpeed * overrallSpeed;

		/* make our standard deviation, substract from our meean to have a 
		 * "sped up" speed for slow units and a "slowed" speed for faster units.
		 * we'll prevent being too slow by taking the speed of the unit with min speed
		 * of the batch if standard deviation is too big. */
		overrallSpeed		= Mathf.Max(overrallSpeed - Mathf.Sqrt(overrallSpeedSqrd),minSpeed);

		for (int i = 0; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.speed = overrallSpeed;
		}
	}

	private void ResetMoveData()
	{
		for (int i = 0; i < _group.Count; i++)
		{
			/* change values of unit  */
			_group[i].NavMeshAgent.velocity		= Vector3.zero;
			_group[i].NavMeshAgent.isStopped	= true;
			_group[i].NavMeshAgent.autoBraking	= true;
			if (_previousSpeeds.Count > i)
				_group[i].NavMeshAgent.speed	= _previousSpeeds[i];
		}

		_previousSpeeds.Clear();
	}

	/*===== Update Methods =====*/

	public void UpdateMovement()
	{
		if (_group.Count > 1 && _moves)
		{
			ComputeUnitsMovement();
		}
	}

	/*===== Movement methods =====*/

	public void Move(Vector3 desiredPos_)
	{
		if (_group.Count <= 0)
			return;

		_desiredPosition = desiredPos_;

		InitMoveData();

		_moves = true;
	}

	private void ComputeUnitsMovement()
	{
		/* infos to get boid vector */
		List<Unit>[] neighbours			= new List<Unit>[_group.Count];
		Vector3[]	 desiredVelocities	= new Vector3[_group.Count];

		/* tests if still moves */
		_moves		= false;
		Vector3 currentPos	= Vector3.zero;
		int count	= 0;

		for (int i = 0; i < _group.Count; i++)//ParallelLoopResult result = Parallel.For(1, _group.Count, (i) => 
		{
			Unit currentUnit = _group[i];
			bool stop = currentUnit.NavMeshAgent.isStopped;

			_moves |= !stop;
			if (stop)
				continue;

			neighbours[i] = new List<Unit>();
			count++;

			for (int j = 0; j < _group.Count; j++)
			{
				Unit testUnit = _group[j];

				Vector3 vecToTest	= testUnit.transform.position - currentUnit.transform.position;
				vecToTest.y			= 0;

				float angle = Mathf.Abs(Vector3.Dot(currentUnit.transform.forward, vecToTest.normalized));

				if (angle <= (Mathf.Deg2Rad * currentUnit.GetUnitData.UnitViewAngle) 
				&& vecToTest.sqrMagnitude <= (currentUnit.GetUnitData.UnitRangeOfSight * currentUnit.GetUnitData.UnitRangeOfSight))
				{
					neighbours[i].Add(testUnit);
				}

			}

			currentPos += currentUnit.transform.position;

			desiredVelocities[i] = ComputeUnitMovement(currentUnit, neighbours[i]);
		}//);

		if (count > 0)
		{
			_currentPos = currentPos / count;
		}
		

		if (Vector3.Distance(_currentPos, _desiredPosition) <= _radius || !_moves)
		{
			_moves = false;
			ResetMoveData();
			return;
		}

		for (int i = 0; i < _group.Count; i++)
		{
			if (_group[i].NavMeshAgent.isStopped)
				_group[i].NavMeshAgent.velocity = Vector3.zero;
			else
				_group[i].NavMeshAgent.velocity = Vector3.ClampMagnitude(desiredVelocities[i] + _group[i].NavMeshAgent.velocity, _group[i].NavMeshAgent.speed);
		}
	}

	private Vector3 ComputeUnitMovement(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 unitDesiredVelocity = Vector3.zero;

		unitDesiredVelocity += unit_.GetUnitData.SquadAlignement * GetAlignement(unit_, neighbours_);
		unitDesiredVelocity += unit_.GetUnitData.SquadCohesion	 * GetCohesion(unit_, neighbours_);
		unitDesiredVelocity += unit_.GetUnitData.SquadSeparation * GetSeparation(unit_, neighbours_);

		return unitDesiredVelocity * unit_.NavMeshAgent.speed - unit_.NavMeshAgent.velocity;
	}

	private Vector3 GetAlignement(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 alignement = Vector3.zero;

		for (int i = 0; i < neighbours_.Count; i++)
		{
			alignement += neighbours_[i].NavMeshAgent.desiredVelocity;
		}
		
		alignement /= neighbours_.Count;
		alignement += unit_.NavMeshAgent.desiredVelocity;
		return alignement;
	}

	private Vector3 GetCohesion(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 cohesion = Vector3.zero;

		for (int i = 0; i < neighbours_.Count; i++)
		{
			cohesion += neighbours_[i].transform.position;
		}

		cohesion /= neighbours_.Count;
		cohesion -= unit_.transform.position;

		return cohesion.normalized;
	}

	private Vector3 GetSeparation(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 separation = Vector3.zero;

		int count = 0;
		for (int i = 0; i < neighbours_.Count; i++)
		{
			Vector3 temp = neighbours_[i].transform.position - unit_.transform.position;
			if (temp.sqrMagnitude <= unit_.GetUnitData.SquadSeparationDist)
			{
				separation += temp;
				count++;
			}
		}

		separation /= count;

		return -separation.normalized;
	}
}
