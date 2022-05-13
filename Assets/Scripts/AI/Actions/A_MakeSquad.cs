using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MakeSquad", menuName = "Actions/Squad")]

public class A_MakeSquad : AIAction
{

    public override bool Execute(Data data)
    {     
        if (data.package.Count <= 0)
        {
            Debug.Log("Bad Size of package");
            return false;
        }

        Squad squad = null;

        Army army = null;
        
        List<int> TypeList = new List<int>();
        List<int>CountsList = new List<int>();
        int TotalUnit = 0;
        
        foreach (var pack in data.package)
        {
            switch (pack.Key)
            {
                case "TypeList":
                    TypeList = (List<int>) pack.Value;

                    break;
                
                case "CountsList":
                    CountsList = (List<int>) pack.Value;

                    break;
                
                case "Army":
                    army = (Army) pack.Value;
                    break;
                default:
                    Debug.LogWarning("bad package");
                    return false;
            }
        }

        if (army == null)
        {
            Debug.Log("army not init");
            return false;
        }
        
        else if (CountsList == null)
        {
            Debug.Log("countsList not init");
            return false;
        }
        
        else if (TypeList == null)
        {
            Debug.Log("TypeList not init");
            return false;
        }
        
        foreach (var value in CountsList)
            TotalUnit += value;

        if (army.UnitList.Count < TotalUnit)
        {
            Debug.Log("Army to small");
            return false;
        }
        
        foreach (var unit in army.UnitList)
        {
            for (int i = 0; i < TypeList.Count; i++)
            {
                if (unit.GetTypeId == TypeList[i])
                {
                    if (CountsList[i] > 0)
                    {
                        squad._group.Add(unit);
                        CountsList[i]--;
                    }
                    
                    break;
                }
            }

            int count = 0;
            foreach (var value in CountsList)
                count += value;

            if (count == 0)
                break;
        }
        
        if (squad._group.Count <= 0)
        {
            Debug.Log("No unit add squad ");
            return false;
        }

        army.AddSquad(squad);

        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }
}
