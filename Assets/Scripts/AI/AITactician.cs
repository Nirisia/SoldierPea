using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public enum EActionType
{
    MakeUnit,
    MakeSquad,
    Build,
    Move,
    
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
    float _personality = 0.5f;
    
    public float Personality => _personality;

    [SerializeField] private float MinimumValue = 0.1f; 
    
    
    public List<AIAction> Actions = new List<AIAction>();
    
    public  List<AIAction> Tactic = new List<AIAction>();
    
    private void Awake()
    {
    }

    private void Start()
    {
    }

    public void SetTactic()
    {
        Tactic.Clear();

        foreach (var action in Actions)
        {
            //action.UpdatePriority(data);
            
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
    public void ExecuteTactic(Data data)
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
    
    public void CreateSquad(in Data data, Army army)
    {
        List<int> typeList = new List<int>();
        typeList.Add(1);
        List<int>countsList = new List<int>();
        countsList.Add(4);
        
        data.package.Add("TypeList", typeList);
        data.package.Add("CountsList", countsList);

    }

    private bool test = true;
    public void ChooseDestination(in Data data, Army army)
    {
        if(test )
        {
            List<Vector3> Pos = new List<Vector3>();
            Pos.Add(new Vector3(50,0,70));
            Pos.Add(new Vector3(0,0,70));
        
            List<Squad> squads = new List<Squad>();
        
            squads.Add(army.SquadList[0]);
            squads.Add(army.SquadList[1]);
            data.package.Add("Squad", squads);
            data.package.Add("Pos", Pos);

            test = false;
        }

        else
        {
            List<Vector3> Pos = new List<Vector3>();
            Pos.Add(new Vector3(-50,0,-70));
            Pos.Add(new Vector3(0,0,-70));
        
            List<Squad> squads = new List<Squad>();
        
            squads.Add(army.SquadList[0]);
            squads.Add(army.SquadList[1]);
            data.package.Add("Squad", squads);
            data.package.Add("Pos", Pos);

        }

        
        
    }
    
}
