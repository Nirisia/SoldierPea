using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakeUnit", menuName = "Actions/MakeUnit")]

public class A_MakeUnit : AIAction
{

	#region Serialized Field
	/*===== Serialized Field =====*/
	
	[SerializeField, Min(0)] private int startCountUnit			= 5;
	[SerializeField, Min(0)] private int armiesUnitDifference	= 7;

	#endregion

	#region Data
	/*===== Data =====*/

	struct A_MakeUnit_Data
	{
		public Army army;
		public Army enemyArmy;
		public int buildPoints;
	}


	private bool UnpackMakeUnitData(out A_MakeUnit_Data makeUnitData_, Data abstractData_)
	{
		makeUnitData_ = new A_MakeUnit_Data();

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "Army":
					makeUnitData_.army = (Army)pack.Value;
					break;

				case "EnemyArmy":
					makeUnitData_.enemyArmy = (Army)pack.Value;
					break;

				case "OwnerBuildPoints":
					makeUnitData_.buildPoints = (int)pack.Value;
					break;
				default:
					Debug.LogWarning("bad package");
					return false;
			}
		}

		if (makeUnitData_.army == null)
		{
			Debug.Log("A_MakeUnit: AIController Army not init");
			return false;
		}
		else if (makeUnitData_.enemyArmy == null)
		{
			Debug.Log("A_MakeUnit: Other Controller Army not init");
			return false;
		}

		return true;
	}

	#endregion

	public override bool Execute(Data data)
    {
        if (data.package.Count != 3)
        {
            Debug.Log("Bad Size of package");
            return false;
        }

        int[] typeCounts = new int[6];

        int totalCount = 0;

        Factory factory = null;
		A_MakeUnit_Data makeUnitData;
		UnpackMakeUnitData(out makeUnitData, data);
        
        if(makeUnitData.army.Cost == 0)
        {
            factory = makeUnitData.army.FactoryList[0];
            typeCounts[0] = startCountUnit;
        }

        if (makeUnitData.enemyArmy.Cost > makeUnitData.army.Cost)
        {
            totalCount = Mathf.Min(makeUnitData.enemyArmy.Cost - makeUnitData.army.Cost, makeUnitData.buildPoints);

			if (totalCount == 0)
			{
				Debug.Log("No construction points");
				return false;
			}

			if (totalCount >= 5)
            {
                for (int i = 0; i < makeUnitData.army.FactoryList.Count; i++)
                {
                    if (makeUnitData.army.FactoryList[i].GetFactoryData.TypeId == 1)
                    {
                        factory = makeUnitData.army.FactoryList[i];
                        break;
                    }
                    else
                    {
                        factory = makeUnitData.army.FactoryList[0];
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

            factory = makeUnitData.army.FactoryList[0];
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

    public override void UpdatePriority(Data data)
    {
		A_MakeUnit_Data makeUnitData;
		UnpackMakeUnitData(out makeUnitData, data);

		_priority = makeUnitData.enemyArmy.Cost != 0 ? Mathf.Min(((float)makeUnitData.enemyArmy.Cost - (float)makeUnitData.army.Cost) / (float)armiesUnitDifference, makeUnitData.buildPoints) : 1.0f;
	}
}
