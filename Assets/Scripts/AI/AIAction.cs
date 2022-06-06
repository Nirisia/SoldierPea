using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;


public abstract class AIAction : ScriptableObject
{
    [Range(0, 1), SerializeField] protected float _priority = 0;

    public float Priority => _priority;
    public EActionType AType = EActionType.None;


    public abstract bool Execute(AIActionData data);
    public abstract void UpdatePriority(AIActionData data);
}

public abstract class AIActionData
{
	public abstract EActionType GetActionType();
}