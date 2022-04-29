using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;


public struct Data
{
    public Dictionary<string, object> package;
}


public abstract class AIAction : ScriptableObject
{
    [Range(0,1)]
    protected float _priority = 0;

    public float Priority => _priority;


    public abstract void Execute(Data data);
    public abstract void UpdatePriority(Data data);

}
