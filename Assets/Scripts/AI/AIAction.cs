using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;





public abstract class AIAction : ScriptableObject
{
    [Range(0,1)]
    protected float _priority = 0;

    public float Priority => _priority;
    public EActionType AType = EActionType.None;


    public abstract bool Execute(Data data);
    public abstract void UpdatePriority(Data data);

}
