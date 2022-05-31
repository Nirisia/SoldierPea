using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public class A_Build_Data : AIActionData
{
    public Factory factory = null;
    public Func<int, Vector3, bool> request = null;
    public Vector3 pos = Vector3.negativeInfinity;
    public int Type = -1;
}


[CreateAssetMenu(fileName = "Build", menuName = "Actions/Build")]
public class A_Build : AIAction
{
    public override bool Execute(AIActionData data)
    {
        if (data is A_Build_Data package)
        {
            if (package.pos == Vector3.negativeInfinity)
            {
                Debug.Log("Factory not initialize");
                return false;
            }
            else if (package.request == null)
            {
                Debug.Log("request not set");
                return false;
            }
            else if (package.Type == -1)
            {
                Debug.Log("unitType bad value");
                return false;
            }

            else if (package.factory == null)
            {
                Debug.Log("factory not init");
                return false;
            }

            if (package.factory.IsBuildingUnit)
                return false;

            package.request(package.Type, package.pos);

            return true;
        }
        else
        {
            return false;
        }
    }


    public override void UpdatePriority(AIActionData data)
    {

    }
}
