using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;

/* a class that represents a group. 
 * Will be used to make move and stop a group of units
 * as Boids. */
public class Squad
{
	/*===== Members =====*/

	public List<Unit> _group	= new List<Unit>();

	public float _viewAngle		= 60.0f;
	public float _rangeOfSight	= 100.0f;

	private Vector3	_desiredPosition	= Vector3.zero;
	public	Vector3	_currentPos			= Vector3.zero;
	private bool	_moves				= false;
	private float	_radius				= 0.0f;

	private List<float> _previousSpeeds = new List<float>();		 


	/*===== Init Methods =====*/

	private void InitMoveData()
	{
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

		/* */
		overrallSpeed		= Mathf.Max(overrallSpeed - Mathf.Sqrt(overrallSpeedSqrd),minSpeed);

		for (int i = 0; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.speed = overrallSpeed;
		}
	}

	/*===== Update Methods =====*/

	public void UpdateMovement()
	{
		if (_group.Count > 0 && _moves)
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
		//_moves = false;
		List<Unit>[]		neighbours			= new List<Unit>[_group.Count];
		Vector3[]			desiredVelocities	= new Vector3[_group.Count];
		_currentPos = Vector3.zero;

		for (int i = 0; i < _group.Count; i++)//ParallelLoopResult result = Parallel.For(1, _group.Count, (i) => 
		{
			Unit currentUnit = _group[i];
			neighbours[i] = new List<Unit>();

			for (int j = 0; j < _group.Count; j++)
			{
				Unit testUnit = _group[j];

				Vector3 vecToTest	= testUnit.transform.position - currentUnit.transform.position;
				vecToTest.y			= 0;

				float angle = Mathf.Abs(Vector3.Dot(currentUnit.transform.forward, vecToTest.normalized));

				if (angle <= (Mathf.Deg2Rad * _viewAngle) && vecToTest.sqrMagnitude <= (_rangeOfSight * _rangeOfSight))
				{
					neighbours[i].Add(testUnit);
				}

			}

			_currentPos += currentUnit.transform.position;

			desiredVelocities[i] = ComputeUnitMovement(currentUnit, neighbours[i]);
		}//);

		_currentPos /= _group.Count;
		

		if (Vector3.Distance(_currentPos, _desiredPosition) <= _radius)
		{
			_moves = false;
			for (int i = 0; i < _group.Count; i++)
			{
				_group[i].NavMeshAgent.velocity	 = Vector3.zero;
				_group[i].NavMeshAgent.isStopped = true;
			}
		
			return;
		}

		for (int i = 0; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.velocity = Vector3.ClampMagnitude(desiredVelocities[i] + _group[i].NavMeshAgent.velocity, _group[i].NavMeshAgent.speed);
		}
	}

	private Vector3 ComputeUnitMovement(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 unitDesiredVelocity = Vector3.zero;

		unitDesiredVelocity += unit_.GetUnitData.SquadAlignement * (GetAlignement(unit_, neighbours_));
		unitDesiredVelocity += unit_.GetUnitData.SquadCohesion * (GetCohesion(unit_, neighbours_)	 );
		unitDesiredVelocity += unit_.GetUnitData.SquadSeparation * (GetSeparation(unit_, neighbours_));

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
		return alignement.normalized;
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
