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

	public Vector3 desiredPosition = Vector3.zero;

	// Update is called once per frame
	public void UpdateMovement()
	{
		if (_group.Count > 0)
		{
			ComputeGroupMovement();
			ComputeUnitsMovement();
		}
	}

	/*===== Movement methods =====*/

	private void ComputeGroupMovement()
	{
		NavMeshPath path = new NavMeshPath();
		_group[0].NavMeshAgent.SetDestination(desiredPosition);
	}

	private void ComputeUnitsMovement()
	{
		List<Unit>[]		neighbours			= new List<Unit>[_group.Count - 1];
		Vector3[]			desiredVelocities	= new Vector3[_group.Count - 1];

		ParallelLoopResult result = Parallel.For(1, _group.Count, (i) => 
		{
			Unit currentUnit = _group[i];
			for (int j = 0; j < _group.Count; j++)
			{
				Unit testUnit = _group[j];
				Vector3 vecToTest = testUnit.transform.position - currentUnit.transform.position;
				vecToTest.y = 0;

				float angle = Mathf.Abs(Vector3.Dot(testUnit.transform.forward, vecToTest.normalized));

				if (angle <= (Mathf.Deg2Rad * _viewAngle) && vecToTest.sqrMagnitude <= (_rangeOfSight * _rangeOfSight))
				{
					neighbours[i - 1].Add(testUnit);
				}

			}

			desiredVelocities[i - 1] = ComputeUnitMovement(currentUnit, neighbours[i - 1]);
		});

		//for (int i = 1; i < _group.Count; i++)
		//{
		//	Unit currentUnit = _group[i];
		//	for (int j = 0; j < _group.Count; j++)
		//	{
		//		Unit testUnit = _group[j];
		//		Vector3 vecToTest = testUnit.transform.position - currentUnit.transform.position;
		//		vecToTest.y = 0;
		//
		//		float angle = Mathf.Abs(Vector3.Dot(testUnit.transform.forward, vecToTest.normalized));
		//		
		//		if (angle <= (Mathf.Deg2Rad * _viewAngle) && vecToTest.sqrMagnitude <= (_rangeOfSight * _rangeOfSight))
		//		{
		//			neighbours[i - 1].Add(testUnit);
		//		}
		//
		//	}
		//
		//	desiredVelocities[i - 1] = ComputeUnitMovement(currentUnit,neighbours[i - 1]);
		//}

		for (int i = 1; i < _group.Count; i++)
		{
			_group[i].NavMeshAgent.destination = _group[i].transform.position + desiredVelocities[i - 1].normalized * _group[i].NavMeshAgent.speed;
		}
	}

	private Vector3 ComputeUnitMovement(Unit unit_, List<Unit> neighbours_)
	{
		Vector3 unitDesiredVelocity = Vector3.zero;

		unitDesiredVelocity += _alignementWeight * GetAlignement(unit_, neighbours_);
		unitDesiredVelocity += _cohesionWeight	 * GetCohesion(unit_, neighbours_);
		unitDesiredVelocity += _separationWeight * GetSeparation(unit_, neighbours_);

		return unitDesiredVelocity;
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
