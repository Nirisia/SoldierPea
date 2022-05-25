using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakeUnit", menuName = "Actions/MakeUnit")]

public class A_MakeUnit : AIAction
{
    [SerializeField] int startCountUnit = 5;
    public override bool Execute(Data data)
    {
        if (data.package.Count != 3)
        {
            Debug.Log("Bad Size of package");
            return false;
        }

        int[] typeCounts = new int[6];

        int totalCount = 0;

        Factory factory = new Factory();

        Army army = null;
        Army enemyArmy = null;
        int buildPoints = 0;
        
        foreach(var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Army":
                    army = (Army)pack.Value;
                    break;

                case "EnemyArmy":
                    enemyArmy = (Army)pack.Value;
                    break;

                case "BuildPoints":
                    buildPoints = (int)pack.Value;
                    break;
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }
        
        if(army.Cost == 0)
        {
            factory = army.FactoryList[0];
            typeCounts[0] = startCountUnit;
        }


        if (enemyArmy.Cost > army.Cost)
        {
            totalCount = Mathf.Min(enemyArmy.Cost - army.Cost, buildPoints);

            if (totalCount >= 5)
            {
                for (int i = 0; i < army.FactoryList.Count; i++)
                {
                    if (army.FactoryList[i].GetFactoryData.TypeId == 1)
                    {
                        factory = army.FactoryList[i];
                        break;
                    }
                    else
                    {
                        factory = army.FactoryList[0];
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
                else
                {
                    for (int c = 2; c >= 0; c--)
                    {
                        typeCounts[c] = totalCount / (c + 1);
                        totalCount %= (c + 1);
                    }
                }
            }
            else if (totalCount > 0)
            {
                factory = army.FactoryList[0];
                for (int c = 2; c >= 0; c--)
                {
                    typeCounts[c] = totalCount / (c + 1);
                    totalCount %= (c + 1);
                }
            }
            else
            {
                Debug.Log("No construction points");
                return false;
            }
        }

        if (!factory)
        {
            Debug.Log("Factory not initialize");
            return false;
        }
        
        for(int i = 0; i < typeCounts.Length; i++)
        {
            factory.RequestUnitBuild(i+1);
        }
           
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
