using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MakeSquad", menuName = "Actions/Squad")]

public class A_MakeSquad : AIAction
{

    public override bool Execute(Data data)
    {     
        if (data.package.Count <= 0)
        {
            Debug.Log("Bad Size of package");
            return false;
        }
        
        Squad squad = new Squad();

        Army army = new Army();
        
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Unit":
                    Unit temp = (Unit)pack.Value;
                    if(temp)
                        squad.Group.Add(temp);

                    break;

                case "Army":
                    army = (Army) pack.Value;
                    break;
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }

        if (army == null)
        {
            Debug.Log("army not init");
            return false;
        }
        else if (squad.Group.Count <= 0)
        {
            Debug.Log("No unit add squad ");
            return false;
        }

        army.AddSquad(squad);
        Debug.Log("Squad execute");

        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
