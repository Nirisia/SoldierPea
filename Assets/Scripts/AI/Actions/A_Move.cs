using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Actions/Move")]

public class A_Move_Data: AIActionData
{
	public override EActionType GetActionType()
	{
		return EActionType.Move;
	}

	public Army myArmy;
	public Army enemyArmy;
	public TargetBuilding[] targetBuilding;
	public int myBuildPoint;
}

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

	#region Execution
	/*====== Execution ======*/

	public override bool Execute(AIActionData data)
    {
	    if (data is A_Move_Data moveData)
	    {
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

				if (moveData.myArmy.SquadList[i].Moving && Vector3.Distance(pos,moveData.myArmy.SquadList[i].DesiredPos) < movementRange)
					return true;

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
		
		for (int captureTarget = 0; captureTarget < priority_Data_.targetBuilding.Length; captureTarget++)
		{
			TargetBuilding targetBuilding = priority_Data_.targetBuilding[captureTarget];
			if(targetBuilding.GetTeam() == priority_Data_.myArmy.Team)
				continue;
			float dist = 1.0f - Mathf.Clamp01((targetBuilding.transform.position - squad.Position).sqrMagnitude / (movementRange * movementRange));

			float temp = Mathf.Clamp01(priority_Data_.myBuildPoint/captureUnWantedBuildPoint) * dist;

			if (temp > captureRatio)
			{
				captureRatio = temp;
				VecTempv = targetBuilding.transform.position;
			}
		}

		pos = VecTempv;
		return captureRatio * captureBuildingWeight;
	}

	float GetAttackFactory(A_Move_Data priority_Data_, Squad squad, out Vector3 pos)
	{
		float Ratio = 0.0f;
		Vector3 VecTempv= Vector3.zero;
		
		for (int enemyFactory = 0; enemyFactory < priority_Data_.enemyArmy.FactoryList.Count; enemyFactory++)
		{
			Factory eFactory = priority_Data_.enemyArmy.FactoryList[enemyFactory];

			float dist = 1.0f - Mathf.Clamp01((eFactory.transform.position - squad.Position).sqrMagnitude / (movementRange * movementRange));

			int ownerStrength = priority_Data_.myBuildPoint + priority_Data_.myArmy.TotalCost();
			int enemyStrength = priority_Data_.enemyArmy._owner.TotalBuildPoints + priority_Data_.enemyArmy.TotalCost();

			float temp= Mathf.Clamp01(ownerStrength - enemyStrength / (ownerStrength + enemyStrength)) * dist;

			if (temp > Ratio)
			{
				Ratio = temp;
				VecTempv = eFactory.transform.position;
			}
		}

		pos = VecTempv;
		return Ratio * attackFactoryWeight;
	}
	
	float GetAttackSquad(A_Move_Data priority_Data_, Squad squad, out Vector3 pos)
	{
		float Ratio = 0.0f;
		Vector3 VecTempv= Vector3.zero;
		
		
		for (int enemySquad = 0; enemySquad < priority_Data_.enemyArmy.SquadList.Count; enemySquad++)
		{
			Squad eSquad		= priority_Data_.enemyArmy.SquadList[enemySquad];

			float dist = 1.0f - Mathf.Clamp01((eSquad.Position - squad.Position).sqrMagnitude/(movementRange * movementRange));

			float ownerCost = squad.GetCost();
			float enemyCost = eSquad.GetCost();

			float temp = Mathf.Clamp01((ownerCost -  enemyCost / (ownerCost + enemyCost))) * dist;

			if (temp > Ratio)
			{
				Ratio = temp;
				VecTempv = eSquad.Position;
			}

		}
		pos = VecTempv;
		return Ratio * squadAttackWeight;
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

			for (int captureTarget = 0; captureTarget < priority_Data_.targetBuilding.Length; captureTarget++)
			{
				TargetBuilding targetBuilding = priority_Data_.targetBuilding[captureTarget];
				
				if(targetBuilding.GetTeam() == priority_Data_.myArmy.Team)
					continue;
				
				float dist = 1.0f - Mathf.Clamp01((targetBuilding.transform.position - ownerSquad.Position).sqrMagnitude / (movementRange * movementRange));

				captureRatio += Mathf.Clamp01((priority_Data_.myBuildPoint/captureUnWantedBuildPoint)) * dist;
			}
		}

		return captureRatio;
	}

	private float GetAttackFactoryRatio(A_Move_Data priority_Data_)
	{
		float attackRatio = 0.0f;

		for (int mySquad = 0; mySquad < priority_Data_.myArmy.SquadList.Count; mySquad++)
		{
			Squad ownerSquad = priority_Data_.myArmy.SquadList[mySquad];

			if (ownerSquad.Count < minUnitInSquad)
				continue;

			for (int enemyFactory = 0; enemyFactory < priority_Data_.enemyArmy.FactoryList.Count; enemyFactory++)
			{
				Factory eFactory = priority_Data_.enemyArmy.FactoryList[enemyFactory];

				float dist = 1.0f - Mathf.Clamp01((eFactory.transform.position - ownerSquad.Position).sqrMagnitude / (movementRange * movementRange));

				int ownerStrength = priority_Data_.myBuildPoint + priority_Data_.myArmy.TotalCost();
				int enemyStrength = priority_Data_.enemyArmy._owner.TotalBuildPoints + priority_Data_.enemyArmy.TotalCost();

				attackRatio += Mathf.Clamp01(ownerStrength - enemyStrength / (ownerStrength + enemyStrength)) * dist;
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

			for (int enemySquad = 0; enemySquad < priority_Data_.enemyArmy.SquadList.Count; enemySquad++)
			{
				Squad eSquad		= priority_Data_.enemyArmy.SquadList[enemySquad];

				float dist = 1.0f - Mathf.Clamp01((eSquad.Position - ownerSquad.Position).sqrMagnitude/(movementRange * movementRange));

				float ownerCost = ownerSquad.GetCost();
				float enemyCost = eSquad.GetCost();

				attackRatio += Mathf.Clamp01(ownerCost -  enemyCost / (ownerCost + enemyCost)) * dist;

			}
		}

		return attackRatio;
	}


	#endregion
}

