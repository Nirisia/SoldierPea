using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Actions/Move")]

public class A_Move : AIAction
{

    public override bool Execute(Data data)
    {
        if (data.package.Count != 2)
        {
            Debug.Log("Bad Size of package");
            return false;
        }
        
        List<Vector3> pos = new List<Vector3>();
        List<Squad> squads = new List<Squad>();
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Squad":
                    squads = (List<Squad>)pack.Value;
                    break;
                
                case "Pos":
                    pos = (List<Vector3>)pack.Value;
                    break;
                
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }

        if (pos.Count <= 0)
        {
            Debug.Log("position not valid");
            return false;
        }
        else if (squads.Count <= 0)
        {
            Debug.Log("squad not set");
            return false;
        }

        else if (squads.Count != pos.Count)
        {
            Debug.Log(" not match enter squad and pos");
            return false;
        }
        
        for (int i = 0; i < squads.Count; i++)
        {
            squads[i].Move(pos[i]);
        }
        
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
