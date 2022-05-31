using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class A_Squad_Data : AIActionData
{
    public Squad squad = new Squad();
    public Army army = null;
    public List<int> TypeList = new List<int>();
    public List<int> CountsList = new List<int>();
    public int TotalUnit = 0;
}

[CreateAssetMenu(fileName = "MakeSquad", menuName = "Actions/Squad")]
public class A_MakeSquad : AIAction
{
    public override bool Execute(AIActionData data)
    {
        if (data is A_Squad_Data package)
        {
            foreach (var value in package.CountsList)
                package.TotalUnit += value;

            if (package.army.UnitList.Count < package.TotalUnit)
            {
                Debug.Log("Army to small");
                return false;
            }

            foreach (var unit in package.army.UnitList)
            {
                for (int i = 0; i < package.TypeList.Count; i++)
                {
                    if (unit.GetTypeId == package.TypeList[i])
                    {
                        if (package.CountsList[i] > 0)
                        {
                            package.squad.Add(unit);
                            package.CountsList[i]--;
                        }

                        break;
                    }
                }

                int count = 0;
                foreach (var value in package.CountsList)
                    count += value;

                if (count == 0)
                    break;
            }

            if (package.squad.Count <= 0)
            {
                Debug.Log("No unit add squad ");
                return false;
            }

            package.army.AddSquad(package.squad);
            Squad test = new Squad();
            test.Add(package.army.UnitList[4]);
            package.army.AddSquad(test);
            Debug.Log("army:" + package.army.SquadList.Count);

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