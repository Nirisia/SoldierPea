using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Actions/Attack")]
public class A_Attack : AIAction
{

    public override bool Execute(Data data)
    {
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
