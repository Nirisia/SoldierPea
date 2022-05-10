using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Conquest", menuName = "Actions/Conquest")]

public class A_Conquest : AIAction
{

    public override bool Execute(Data data)
    {
        return true;

    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
