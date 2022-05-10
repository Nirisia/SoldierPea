using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public enum EActionType
{
    MakeUnit,
    Attack,
    Build,
    Conquest,
    Defend,
    Move,
    Wait,
    None
}

public class Data
{
    public Data()
    {
        package = new Dictionary<string, object>();
    }
    public Dictionary<string, object> package;
    
    
}

public class AITactician : MonoBehaviour
{
    

    [Range(0,1)]
    float Personality = 0.5f;
    
    public List<AIAction> Actions = new List<AIAction>();
    
    private List<AIAction> Tactic = new List<AIAction>();
    private UtilitySystem US;


    private void Awake()
    {
        SetTactic();
    }

    private void Start()
    {

    }


    public void SetTactic()
    {
        foreach (var action in Actions)
        {
            if (action.AType == EActionType.Build)
            {
                Tactic.Add(action);          
            }
        }
        
    }
    public void ExecuteTactic(Data data)
    {
        GetNextAction().Execute(data);
        Tactic.Remove(GetNextAction());
    }


    public AIAction GetNextAction()
    {
        return Tactic[0];
    }


    public void ChooseTypeAndCountUnit(in Data data)
    {
        data.package.Add("Count",5);
        data.package.Add("Type", 1);
    }

    public void ChooseTypeAndPosFactory(in Data data)
    {
        data.package.Add("Type", 0);
        Vector3 pos = new Vector3(20,0,0);
        data.package.Add("Pos", pos);
        
    }
    
}
