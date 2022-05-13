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
        
        Squad squad = new Squad();
        Unit unit = new Unit();
        Vector3 pos = Vector3.negativeInfinity;

        bool CanMove = false;
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Squad":
                    squad = (Squad)pack.Value;
                    CanMove = true;
                    break;
                
                case "Unit":
                    unit = (Unit)pack.Value;
                    CanMove = true;
                    break;
                case "Pos":
                    pos = (Vector3)pack.Value;
                    break;
                
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }

        if (pos == Vector3.negativeInfinity)
        {
            Debug.Log("Factory not initialize");
            return false;
        }
        else if (squad == null && !CanMove)
        {
            Debug.Log("squad not set");
            return false;
        }
        else if (unit == null && !CanMove)
        {
            Debug.Log("unit not set");
            return false;
        }
        
        if(CanMove && squad == null)
            squad.Move(pos);
        else if(CanMove && unit)
            unit.SetTargetPos(pos);
        
        Debug.Log("Move execute");
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
