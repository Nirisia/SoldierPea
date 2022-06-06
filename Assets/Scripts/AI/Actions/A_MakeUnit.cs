using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakeUnit", menuName = "Actions/MakeUnit")]

public class A_MakeUnit_Data: AIActionData
{
	public override EActionType GetActionType()
	{
		return EActionType.MakeUnit;
	}

	public Army army;
	public Army enemyArmy;
	public int buildPoints;
}

public class A_MakeUnit : AIAction
{

	#region Serialized Field
	/*===== Serialized Field =====*/
	
	[SerializeField, Min(0)] private int startCountUnit			= 5;
	[SerializeField, Min(0)] private int armiesUnitDifference	= 7;

	#endregion

	public override bool Execute(AIActionData data)
    {

	    int[] typeCounts = new int[6];

        int totalCount = 0;

        Factory factory = null;
		
		//UnpackMakeUnitData(out makeUnitData, data);
		if (data is A_MakeUnit_Data package)
		{


			if (package.army.Cost == 0)
			{
				factory = package.army.FactoryList[0];
				typeCounts[0] = startCountUnit;
			}

			if (package.enemyArmy.Cost > package.army.Cost)
			{
				totalCount = Mathf.Min(package.enemyArmy.Cost - package.army.Cost, package.buildPoints);

				if (totalCount == 0)
				{
					Debug.Log("No construction points");
					return false;
				}

				if (totalCount >= 5)
				{
					for (int i = 0; i < package.army.FactoryList.Count; i++)
					{
						if (package.army.FactoryList[i].GetFactoryData.TypeId == 1)
						{
							factory = package.army.FactoryList[i];
							break;
						}
						else
						{
							factory = package.army.FactoryList[0];
						}
					}

					if (factory.GetFactoryData.TypeId == 1)
					{
						for (int c = 5; c >= 3; c--)
						{
							typeCounts[c] = totalCount / (c + 1);
							totalCount %= (c + 1);
						}
					}
				}

				factory = package.army.FactoryList[0];
				for (int c = 2; c >= 0; c--)
				{
					typeCounts[c] = totalCount / (c + 1);
					totalCount %= (c + 1);
				}
			}

			if (!factory)
			{
				Debug.Log("Factory not initialize");
				return false;
			}

			for (int i = 0; i < typeCounts.Length; i++)
			{
				for (int j = 0; j < typeCounts[i]; j++)
				{
					factory.RequestUnitBuild(i);
				}
			}


			return true;
		}
		else
			return false;
    }

    public override void UpdatePriority(AIActionData data)
    {
	    if (data is A_MakeUnit_Data package)
		{
			_priority = Mathf.Clamp01(package.army.Cost != 0
				? Mathf.Min(((float) package.enemyArmy.Cost - (float) package.army.Cost) / (float) armiesUnitDifference,
				package.buildPoints)
				: 1.0f);
		}
    }
}
