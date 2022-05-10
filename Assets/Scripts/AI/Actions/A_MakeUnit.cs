using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakeUnit", menuName = "Actions/MakeUnit")]

public class A_MakeUnit : AIAction
{
    public override bool Execute(Data data)
    {
        if (data.package.Count != 3)
        {
            Debug.Log("Bad Size of package");
            return false;
        }
        
        Factory factory = new Factory();
        int count = 0;
        int UnitType = -1;
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Factory":
                    factory = (Factory)pack.Value;
                    break;
                
                case "Count":
                    count = (int) pack.Value;
                    break;
                
                case "Type":
                    UnitType = (int) pack.Value;
                    break;
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }

        if (!factory)
        {
            Debug.Log("Factory not initialize");
            return false;
        }
        else if(count == 0)
        {
            Debug.Log("count = 0");
            return false;
        }
        else if (UnitType == -1)
        {
            Debug.Log("unitType bad value");
            return false;
        }


        
        for(int i = 0; i < count; i++)
            factory.RequestUnitBuild(UnitType);
        
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
