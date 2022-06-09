using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public enum EActionType
{
	None,
	MakeUnit,
    MakeSquad,
    Build,
    Move,
    
	Count
}
public class AITactician : MonoBehaviour
{
    

    [Range(0,1)]
    float _personality = 0.5f;
    
    public float Personality => _personality;

    [SerializeField] private float MinimumValue = 0.1f; 
    
    
    public List<AIAction> Actions = new List<AIAction>();
    
    public  List<AIAction> Tactic = new List<AIAction>();
    
    public void SetTactic(List<AIActionData> sortData)
    {
        Tactic.Clear();

        foreach (var action in Actions)
        {
			action.UpdatePriority(sortData.First(e => action.AType == e.GetActionType()));
            
            if (action.Priority >= MinimumValue)
            {
                if(Tactic.Count == 0)
                    Tactic.Add(action);
                else
                    SortAction(action);
            }
            
        }

    }

    void SortAction(AIAction action)
    {
        for (int i = 0; i < Tactic.Count; i++)
        {
            if (Tactic[i].Priority < action.Priority)
            {
                Tactic.Insert(i, action);
                return;
            }

        }
        Tactic.Add(action);
    }
    AIAction SelectAction(EActionType type)
    {
        foreach (var action in Actions)
        {
            if (action.AType == type)
            {
                return action;
            }
        }

        Debug.LogError("Not action found");
        return null;
    }
    public void ExecuteTactic(AIActionData data)
    {
        if (GetNextAction())
        {
            if(GetNextAction().Execute(data))
                Tactic.Remove(GetNextAction());
        }
    }


    public AIAction GetNextAction()
    {
        if (Tactic.Count <= 0)
            return null;
        
        return Tactic[0];
    }
}
