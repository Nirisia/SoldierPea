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

        int countType0 = 0;
        int countType1 = 0;
        int countType2 = 0;
        int countType3 = 0;
        int countType4 = 0;
        int countType5 = 0;

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
            countType0 = startCountUnit;
        }


        if (enemyArmy.Cost > army.Cost)
        {
            totalCount = enemyArmy.Cost - army.Cost;

            if (buildPoints >= 5)
            {
                for (int i = 0; i < army.FactoryList.Count; i++)
                {
                    if (army.FactoryList[i].GetFactoryData.TypeId == 1)
                    {
                        factory = army.FactoryList[i];
                        break;
                    }
                }
                factory = army.FactoryList[0];

            }
        }

        if (!factory)
        {
            Debug.Log("Factory not initialize");
            return false;
        }
        
        for(int i = 0; i < countType0; i++)
            factory.RequestUnitBuild(0);

        for (int i = 0; i < countType1; i++)
            factory.RequestUnitBuild(1);

        for (int i = 0; i < countType2; i++)
            factory.RequestUnitBuild(2);

        for (int i = 0; i < countType3; i++)
            factory.RequestUnitBuild(3);

        for (int i = 0; i < countType4; i++)
            factory.RequestUnitBuild(4);

        for (int i = 0; i < countType5; i++)
            factory.RequestUnitBuild(5);

        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
