using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "Build", menuName = "Actions/Build")]

public class A_Build : AIAction
{
	#region Serialized Field
	/*===== Serialized Field =====*/

	[SerializeField, Min(0)] private int typeFactory = 0;

	#endregion

	#region Data
	/*===== Data =====*/

	struct A_Build_Data
	{
		public Army army;
		public Army enemyArmy;
		public TargetBuilding[] targetBuilding;
		public int	buildPoints;
		public Func<int, Vector3, bool> request;
	}

	private bool UnpackBuildData(out A_Build_Data buildData_, Data abstractData_)
	{
		buildData_ = new A_Build_Data();

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "Army":
					buildData_.army = (Army)pack.Value;
					break;

				case "EnemyArmy":
					buildData_.enemyArmy = (Army)pack.Value;
					break;

				case "OwnerBuildPoints":
					buildData_.buildPoints = (int)pack.Value;
					break;

				case "TargetBuildings":
					buildData_.targetBuilding = (TargetBuilding[])pack.Value;
					break;

				case "Request":
					buildData_.request = (Func<int, Vector3, bool>)pack.Value;
					break;
				default:
					Debug.LogWarning("bad package");
					return false;
			}
		}

		if (buildData_.army == null)
		{
			Debug.Log("A_Build: AIController Army not init");
			return false;
		}
		else if (buildData_.enemyArmy == null)
		{
			Debug.Log("A_Build: Other Controller Army not init");
			return false;
		}
		else if (buildData_.request == null)
		{
			Debug.Log("A_Build: Other Controller Request not init");
			return false;
		}
		else if (buildData_.targetBuilding == null)
		{
			Debug.Log("A_Build: Target Buildings not init");
			return false;
		}

		return true;
	}

	#endregion

	public override bool Execute(Data data)
    {
        if (data.package.Count != 4)
        {
            Debug.Log("Bad Size of package");
            return false;
        }

		A_Build_Data buildData;
		UnpackBuildData(out buildData, data);

		Factory factory = buildData.army.FactoryList[0];
		Vector3 armyPos = buildData.army.transform.position;

        if (factory.IsBuildingUnit)
            return false;

		Vector3 pos = Vector3.zero;

		float dist = float.MaxValue;
		for (int i = 0; i < buildData.targetBuilding.Length; i++)
		{
			Vector3 iTargetPos = buildData.targetBuilding[i].transform.position;
			float iDist = (armyPos - iTargetPos).sqrMagnitude;

			if (iDist < dist)
			{
				dist = iDist;
				pos = iTargetPos;
			}
		}
        //buildData.request(typeFactory, pos);

        return true;    
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
