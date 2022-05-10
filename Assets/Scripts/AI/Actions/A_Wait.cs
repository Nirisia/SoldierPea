using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wait", menuName = "Actions/Wait")]

public class A_Wait : AIAction
{

    public override bool Execute(Data data)
    {
        return true;
       
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
