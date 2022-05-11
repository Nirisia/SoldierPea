using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;

/* a class that represents a group. 
 * Will be used to make move and stop a group of units
 * as Boids. */
public class Squad : MonoBehaviour
{
	public List<Unit> _group = new List<Unit>();

	public float _alignementWeight		= 0.5f;
	public float _cohesionWeight		= 0.5f;
	public float _separationWeight		= 0.5f;

	public float _viewAngle		= 60.0f;
	public float _rangeOfSight	= 100.0f;

	public Vector3	_desiredPosition = Vector3.zero;
	public bool		_moves = false;

	// Update is called once per frame
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
		_desiredPosition = desiredPos_;

		for (int i = 0; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.isStopped = false;
			_group[i].NavMeshAgent.SetDestination(_desiredPosition);
		}

		_moves = true;
	}

	private void ComputeUnitsMovement()
	{
		_moves = false;
		List<Unit>[]		neighbours			= new List<Unit>[_group.Count];
		Vector3[]			desiredVelocities	= new Vector3[_group.Count];

		for (int i = 0; i < _group.Count; i++)//ParallelLoopResult result = Parallel.For(1, _group.Count, (i) => 
		{
			Unit currentUnit = _group[i];
			neighbours[i] = new List<Unit>();

			currentUnit.NavMeshAgent.isStopped = currentUnit.NavMeshAgent.remainingDistance <= currentUnit.NavMeshAgent.stoppingDistance;
			_moves |= !currentUnit.NavMeshAgent.isStopped;
			if (currentUnit.NavMeshAgent.isStopped)
				continue;

			RaycastHit hit;
			if (Physics.Raycast(currentUnit.transform.position + Vector3.up,currentUnit.transform.forward, out hit, _rangeOfSight))
			{
				NavMeshAgent agent = hit.collider.GetComponent<NavMeshAgent>();

				Debug.Log(hit.collider.name);

				if (agent && agent.isStopped)
					currentUnit.NavMeshAgent.isStopped = true;
			}


			for (int j = 0; j < _group.Count; j++)
			{
				Unit testUnit		= _group[j];
				Vector3 vecToTest	= testUnit.transform.position - currentUnit.transform.position;
				vecToTest.y			= 0;

				float angle = Mathf.Abs(Vector3.Dot(currentUnit.transform.forward, vecToTest.normalized));

				if (angle <= (Mathf.Deg2Rad * _viewAngle) && vecToTest.sqrMagnitude <= (_rangeOfSight * _rangeOfSight))
				{
					neighbours[i].Add(testUnit);
				}

			}

			desiredVelocities[i] = ComputeUnitMovement(currentUnit, neighbours[i]);
		}//);

		for (int i = 0; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.velocity = Vector3.ClampMagnitude(_group[i].NavMeshAgent.desiredVelocity + _group[i].NavMeshAgent.velocity + desiredVelocities[i], _group[i].NavMeshAgent.speed);
		}
	}

	private Vector3 ComputeUnitMovement(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 unitDesiredVelocity = Vector3.zero;

		unitDesiredVelocity += _alignementWeight * GetAlignement(unit_, neighbours_);
		unitDesiredVelocity += _cohesionWeight	 * GetCohesion(unit_, neighbours_);
		unitDesiredVelocity += _separationWeight * GetSeparation(unit_, neighbours_);

		return unitDesiredVelocity * unit_.NavMeshAgent.speed;
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

		for (int i = 0; i < neighbours_.Count; i++)
		{
			separation += neighbours_[i].transform.position - unit_.transform.position;
		}

		separation /= neighbours_.Count;

		return -separation.normalized;
	}
}
