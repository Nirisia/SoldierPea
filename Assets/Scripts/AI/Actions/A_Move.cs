using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Actions/Move")]
public class A_Move : AIAction
{

	#region Serialized Field
	/*===== Serialized Field =====*/

	[SerializeField, Range(0.0f, 1.0f)] private float squadAttackWeight = 0.3f;
	[SerializeField, Range(0.0f, 1.0f)] private float attackFactoryWeight = 0.3f;
	[SerializeField, Range(0.0f, 1.0f)] private float captureBuildingWeight = 0.3f;

	#endregion

	#region Execution
	/*====== Execution ======*/

	struct A_Move_Data
	{
		public Army				myArmy;
		public Army				enemyArmy;
		public TargetBuilding[]	targetBuilding;
	}

    public override bool Execute(Data data)
    {
		A_Move_Data moveData;
		UnpackMoveData(out moveData, data);


		for (int i = 0; i < moveData.myArmy.SquadList.Count; i++)
        {
			//moveData.myArmy.SquadList[i].Move(pos[i]);
        }
        
        return true;
    }

	private bool UnpackMoveData(out A_Move_Data moveData_, Data abstractData_)
	{
		moveData_ = new A_Move_Data();
		if (abstractData_.package.Count != 3)
		{
			Debug.Log("A_Move: bad package size");
			return false;
		}

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "OwnerArmy":
					moveData_.myArmy	= (Army)pack.Value;
					break;

				case "EnemyArmy":
					moveData_.enemyArmy = (Army)pack.Value;
					break;

				case "TargetBuildings":
					moveData_.targetBuilding = (TargetBuilding[])pack.Value;
					break;

				default:
					Debug.LogWarning("bad package");
					return false;
			}
		}

		if (moveData_.myArmy == null)
		{
			Debug.Log("A_Move: AIController Army not init");
			return false;
		}
		else if (moveData_.enemyArmy == null)
		{
			Debug.Log("A_Move: Other Controller Army not init");
			return false;
		}
		else if (moveData_.enemyArmy == null)
		{
			Debug.Log("A_Move: Other Controller Army not init");
			return false;
		}

		return true;
	}

	#endregion

	#region Priority
	/*=====  Priority  =====*/

	struct A_MovePriority_Data
	{
		public Army myArmy;
		public Army enemyArmy;
		public TargetBuilding[] targetBuilding;
		public int myBuildPoint;
	}

	public override void UpdatePriority(Data data)
	{
		A_MovePriority_Data moveData;
		UnpackMovePriorityData(out moveData, data);


	}

	private bool UnpackMovePriorityData(out A_MovePriority_Data moveData_, Data abstractData_)
	{
		moveData_ = new A_MovePriority_Data();

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "OwnerArmy":
					moveData_.myArmy = (Army)pack.Value;
					break;

				case "EnemyArmy":
					moveData_.enemyArmy = (Army)pack.Value;
					break;

				case "TargetBuildings":
					moveData_.targetBuilding = (TargetBuilding[])pack.Value;
					break;

				case "OwnerBuildPoint":
					moveData_.myBuildPoint = (int)pack.Value;
					break;

				default:
					Debug.LogWarning("bad package");
					return false;
			}
		}

		if (moveData_.myArmy == null)
		{
			Debug.Log("A_Move: AIController Army not init");
			return false;
		}
		else if (moveData_.enemyArmy == null)
		{
			Debug.Log("A_Move: Other Controller Army not init");
			return false;
		}
		else if (moveData_.enemyArmy == null)
		{
			Debug.Log("A_Move: Other Controller Army not init");
			return false;
		}

		return true;
	}

	#endregion
}

