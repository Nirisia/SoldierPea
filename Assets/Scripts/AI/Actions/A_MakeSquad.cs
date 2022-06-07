using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class A_Squad_Data : AIActionData
{
	public override EActionType GetActionType()
	{
		return EActionType.MakeSquad;
	}

	public Squad squad = new Squad();
    public Army army = null;
    public Army EnemyArmy = null;
    public TargetBuilding[] targets = null;

}

class Possible_Target
{
    public TargetBuilding target = null;
    public float dist = 0.0f;
    public int TargetFind = 0; 

    public void Set(TargetBuilding _target, float _dist)
    {
        target = _target;
        dist = _dist;
        TargetFind = 1;
    }
}

[CreateAssetMenu(fileName = "MakeSquad", menuName = "Actions/Squad")]
public class A_MakeSquad : AIAction
{
    
    #region Serialized Field
    /*===== Serialized Field =====*/

    [SerializeField, Range(0.0f, 1.0f)] private float squadValueWeight = 0.3f;
    [SerializeField, Range(0.0f, 1.0f)] private float costWeight = 0.3f;
    [SerializeField, Range(0.0f, 1.0f)] private float distWeight = 0.3f;
    [SerializeField] private float Radius = 1f;
    [SerializeField, Range(0.0f, 1.0f)] private float Seuil = 0.7f;
    
    
    #endregion
    
    public override bool Execute(AIActionData data)
    {
        if (data is A_Squad_Data package)
        {
            List<Possible_Target> PossibleAttack = new List<Possible_Target>();
            foreach (var unit in package.army.UnitList)
            {
                Possible_Target targetActu = new Possible_Target();
                foreach (var building in package.targets)
                {
                    float dist = (unit.transform.position - building.transform.position).magnitude;
                    if (dist <= Radius)
                    {
                        if (targetActu.TargetFind == 0)
                            targetActu.Set(building, dist);
                        else if (targetActu.dist > dist)
                            targetActu.Set(building, dist);
                        
                        PossibleAttack.Add(targetActu);
                    }
                }
            }
            
            
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
        if (data is A_Squad_Data package)
        {
            int TotalUnit = 0;
            foreach (var factory in package.army.FactoryList)
            {
                if (factory.IsBuildingUnit)
                {
                    TotalUnit += factory.UnitInQueue;
                }
            }

            TotalUnit += package.army.UnitList.Count;
            int MedCost = 0;
            if (TotalUnit > 0)
            {
                MedCost = package.army.TotalCost() / TotalUnit;
                foreach (var squad in package.army.SquadList)
                {
                    if (squad.GetCost() < MedCost)
                    {
                        _priority = Seuil;
                        return;
                    }
                }

                _priority = 0;
            }
            else
                _priority = 0;
        }

    }
}
