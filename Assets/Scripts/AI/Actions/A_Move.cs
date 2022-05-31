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

	[SerializeField] private float movementRange = 100.0f;
	[SerializeField,Min(0)] private int captureUnWantedBuildPoint = 10;
	[SerializeField,Min(1)] private int minUnitInSquad = 3;


	#endregion

	#region Data
	/*====== Data ======*/

	public class A_Move_Data: AIActionData
	{
		public Army myArmy;
		public Army enemyArmy;
		public TargetBuilding[] targetBuilding;
		public int myBuildPoint;
	}

	private bool UnpackMoveData(out A_Move_Data moveData_, Data abstractData_)
	{
		moveData_ = new A_Move_Data();

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "Army":
					moveData_.myArmy = (Army)pack.Value;
					break;

				case "EnemyArmy":
					moveData_.enemyArmy = (Army)pack.Value;
					break;

				case "TargetBuildings":
					moveData_.targetBuilding = (TargetBuilding[])pack.Value;
					break;

				case "OwnerBuildPoints":
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
		else if (moveData_.targetBuilding == null)
		{
			Debug.Log("A_Move: Target Buildings not init");
			return false;
		}

		return true;
	}

	#endregion

	#region Execution
	/*====== Execution ======*/

	public override bool Execute(AIActionData data)
    {
	    if (data is A_Move_Data moveData)
	    {
			//UnpackMoveData(out moveData, data);
		    float priority = 0;
		    Vector3 pos = Vector3.zero;

		    for (int i = 0; i < moveData.myArmy.SquadList.Count; i++)
		    {
			    Vector3 vecTemp = Vector3.zero;
			    SelectPos(ref priority, GetCapturePos(moveData, moveData.myArmy.SquadList[i], out vecTemp), ref pos,
				    vecTemp);
			    SelectPos(ref priority, GetAttackFactory(moveData, moveData.myArmy.SquadList[i], out vecTemp), ref pos,
				    vecTemp);
			    SelectPos(ref priority, GetAttackSquad(moveData, moveData.myArmy.SquadList[i], out vecTemp), ref pos,
				    vecTemp);
			    moveData.myArmy.SquadList[i].Move(pos);
		    }

		    return true;
	    }
	    else
		    return false;
    }

	void SelectPos(ref float priority, float value, ref Vector3 pos, Vector3 temp)
	{
		if (value > priority)
		{
			priority = value;
			pos = temp;
		}
	}
	#endregion

	#region Priority
	/*=====  Priority  =====*/


	float GetCapturePos(A_Move_Data priority_Data_, Squad squad, out Vector3 pos)
	{
		float captureRatio = 0.0f;
		Vector3 VecTempv= Vector3.zero;
		float squadStrength = Mathf.Clamp01(squad.Count - minUnitInSquad / (squad.Count + minUnitInSquad));
		
		for (int captureTarget = 0; captureTarget < priority_Data_.targetBuilding.Length; captureTarget++)
		{
			TargetBuilding targetBuilding = priority_Data_.targetBuilding[captureTarget];
			if(targetBuilding.GetTeam() == priority_Data_.myArmy.Team)
				continue;
			float dist = Mathf.Clamp01(1.0f - (targetBuilding.transform.position - squad._currentPos).sqrMagnitude / (movementRange * movementRange));

			float temp = Mathf.Clamp01((float)priority_Data_.myBuildPoint/(float)captureUnWantedBuildPoint + squadStrength) * dist;

			if (temp > captureRatio)
			{
				captureRatio = temp;
				VecTempv = targetBuilding.transform.position;
			}
		}

		pos = VecTempv;
		return captureRatio;
	}

	float GetAttackFactory(A_Move_Data priority_Data_, Squad squad, out Vector3 pos)
	{
		float Ratio = 0.0f;
		Vector3 VecTempv= Vector3.zero;
		float squadStrength = Mathf.Clamp01(squad.Count - minUnitInSquad / (squad.Count + minUnitInSquad));
		float ownerUnitNb = (float)priority_Data_.myArmy.UnitList.Count;
		float enemyUnitNb = (float)priority_Data_.enemyArmy.UnitList.Count;
		
		
		for (int enemyFactory = 0; enemyFactory < priority_Data_.enemyArmy.SquadList.Count; enemyFactory++)
		{
			Factory eFactory = priority_Data_.enemyArmy.FactoryList[enemyFactory];

			float dist = Mathf.Clamp01(1.0f - (eFactory.transform.position - squad._currentPos).sqrMagnitude / (movementRange * movementRange));

			
			float temp= (Mathf.Clamp01(ownerUnitNb - enemyUnitNb / (ownerUnitNb + enemyUnitNb)) + squadStrength) * dist;

			if (temp > Ratio)
			{
				Ratio = temp;
				VecTempv = eFactory.transform.position;
			}
		}

		pos = VecTempv;
		return Ratio;
	}
	
	float GetAttackSquad(A_Move_Data priority_Data_, Squad squad, out Vector3 pos)
	{
		float Ratio = 0.0f;
		Vector3 VecTempv= Vector3.zero;
		float squadStrength = Mathf.Clamp01(squad.Count - minUnitInSquad / (squad.Count + minUnitInSquad));
		
		
		for (int enemySquad = 0; enemySquad < priority_Data_.enemyArmy.SquadList.Count; enemySquad++)
		{
			Squad eSquad		= priority_Data_.enemyArmy.SquadList[enemySquad];

			float dist = Mathf.Clamp01(1.0f - (eSquad._currentPos - squad._currentPos).sqrMagnitude/(movementRange * movementRange));

			float ownerCost = (float)squad.GetCost();
			float enemyCost = (float)eSquad.GetCost();

			float temp = (Mathf.Clamp01(ownerCost -  enemyCost / (ownerCost + enemyCost))  + squadStrength) * dist;

			if (temp > Ratio)
			{
				Ratio = temp;
				VecTempv = eSquad._currentPos;
			}

		}
		pos = VecTempv;
		return Ratio;
	}

	public override void UpdatePriority(AIActionData data_)
	{
		if (data_ is A_Move_Data moveData)
		{
			//UnpackMoveData(out moveData, data_);
			_priority = Mathf.Clamp01(GetAttackFactoryRatio(moveData) * attackFactoryWeight
			                          + GetAttackSquadRatio(moveData) * squadAttackWeight
			                          + GetCaptureFactoryRatio(moveData) * captureBuildingWeight);

		}
	}

	private float GetCaptureFactoryRatio(A_Move_Data priority_Data_)
	{
		float captureRatio = 0.0f;

		for (int mySquad = 0; mySquad < priority_Data_.myArmy.SquadList.Count; mySquad++)
		{
			Squad ownerSquad = priority_Data_.myArmy.SquadList[mySquad];

			if (ownerSquad.Count < minUnitInSquad)
				continue;

			float squadStrength = Mathf.Clamp01(ownerSquad.Count - minUnitInSquad / (ownerSquad.Count + minUnitInSquad));

			for (int captureTarget = 0; captureTarget < priority_Data_.targetBuilding.Length; captureTarget++)
			{
				TargetBuilding targetBuilding = priority_Data_.targetBuilding[captureTarget];
				
				if(targetBuilding.GetTeam() == priority_Data_.myArmy.Team)
					continue;
				
				float dist = Mathf.Clamp01(1.0f - (targetBuilding.transform.position - ownerSquad._currentPos).sqrMagnitude / (movementRange * movementRange));

				captureRatio += Mathf.Clamp01((float)priority_Data_.myBuildPoint/(float)captureUnWantedBuildPoint + squadStrength) * dist;
			}
		}

		return captureRatio;
	}

	private float GetAttackFactoryRatio(A_Move_Data priority_Data_)
	{
		float attackRatio = 0.0f;

		float ownerUnitNb = (float)priority_Data_.myArmy.UnitList.Count;
		float enemyUnitNb = (float)priority_Data_.enemyArmy.UnitList.Count;

		for (int mySquad = 0; mySquad < priority_Data_.myArmy.SquadList.Count; mySquad++)
		{
			Squad ownerSquad = priority_Data_.myArmy.SquadList[mySquad];

			if (ownerSquad.Count < minUnitInSquad)
				continue;

			float squadStrength = Mathf.Clamp01(ownerSquad.Count - minUnitInSquad / (ownerSquad.Count + minUnitInSquad));

			for (int enemyFactory = 0; enemyFactory < priority_Data_.enemyArmy.SquadList.Count; enemyFactory++)
			{
				Factory eFactory = priority_Data_.enemyArmy.FactoryList[enemyFactory];

				float dist = Mathf.Clamp01(1.0f - (eFactory.transform.position - ownerSquad._currentPos).sqrMagnitude / (movementRange * movementRange));

				attackRatio += (Mathf.Clamp01(ownerUnitNb - enemyUnitNb / (ownerUnitNb + enemyUnitNb)) + squadStrength) * dist;
			}
		}

		return attackRatio;
	}

	private float GetAttackSquadRatio(A_Move_Data priority_Data_)
	{
		float attackRatio = 0.0f;

		for (int mySquad = 0; mySquad < priority_Data_.myArmy.SquadList.Count; mySquad++)
		{
			Squad ownerSquad = priority_Data_.myArmy.SquadList[mySquad];

			if (ownerSquad.Count < minUnitInSquad)
				continue;

			float squadStrength = Mathf.Clamp01(ownerSquad.Count - minUnitInSquad / (ownerSquad.Count + minUnitInSquad));

			for (int enemySquad = 0; enemySquad < priority_Data_.enemyArmy.SquadList.Count; enemySquad++)
			{
				Squad eSquad		= priority_Data_.enemyArmy.SquadList[enemySquad];

				float dist = Mathf.Clamp01(1.0f - (eSquad._currentPos - ownerSquad._currentPos).sqrMagnitude/(movementRange * movementRange));

				float ownerCost = (float)ownerSquad.GetCost();
				float enemyCost = (float)eSquad.GetCost();

				attackRatio += (Mathf.Clamp01(ownerCost -  enemyCost / (ownerCost + enemyCost))  + squadStrength) * dist;

			}
		}

		return attackRatio;
	}


	#endregion
}

