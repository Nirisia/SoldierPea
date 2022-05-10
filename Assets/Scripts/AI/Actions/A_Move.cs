using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Actions/Move")]

public class A_Move : AIAction
{

    public override bool Execute(Data data)
    {        
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
