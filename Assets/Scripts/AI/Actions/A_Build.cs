using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "Build", menuName = "Actions/Build")]

public class A_Build : AIAction
{

    public override bool Execute(Data data)
    {
        if (data.package.Count != 3)
        {
            Debug.Log("Bad Size of package");
            return false;
        }
        
        Factory factory = new Factory();
        Func<int, Vector3, bool> request = null;
        Vector3 pos = Vector3.negativeInfinity;
        int Type = -1;
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "Request":
                    request = (Func<int, Vector3, bool>)pack.Value;
                    break;
                
                case "Pos":
                    pos = (Vector3)pack.Value;
                    break;

                case "Type":
                    Type = (int) pack.Value;
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
        else if (request == null)
        {
            Debug.Log("request not set");
            return false;
        }
        else if (Type == -1)
        {
            Debug.Log("unitType bad value");
            return false;
        }

        request(Type, pos);
        Debug.Log("Build execute");

        return true;    
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
