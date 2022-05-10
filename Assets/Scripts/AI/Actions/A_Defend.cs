using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Defend", menuName = "Actions/Defend")]

public class A_Defend : AIAction
{
    public override bool Execute(Data data)
    {
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
