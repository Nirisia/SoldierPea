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
			if (package.army.UnitList.Count == 0)
				return false;

			List<Unit> availableUnit		= new List<Unit>(package.army.UnitList);
			List<Vector3> avgDesiredUnitPos = new List<Vector3>();


			for (int i = 0;  i < package.army.UnitList.Count; i++)
			{
				Unit iUnit = package.army.UnitList[i];
				Vector3 unitAvgPos = Vector3.zero;

				/* add pos of closest point of interest */
				if (package.EnemyArmy.SquadList.Count > 0)
					unitAvgPos += package.EnemyArmy.SquadList.OrderBy(e => (e.Position - iUnit.transform.position).sqrMagnitude).First().Position;
				if (package.EnemyArmy.FactoryList.Count > 0)
					unitAvgPos += package.EnemyArmy.FactoryList.OrderBy(e => (e.transform.position - iUnit.transform.position).sqrMagnitude).First().transform.position;
				if (package.targets.Length > 0)
					unitAvgPos += package.targets.OrderBy(e => (e.transform.position - iUnit.transform.position).sqrMagnitude).First().transform.position;

				/* compute the avg desired pos (we don't care if it is not the real pos,
				 * we're just searching the closest point to each unit for those available) */
				unitAvgPos /= 3;

				avgDesiredUnitPos.Add(unitAvgPos);
			}

			int avgCost = package.army.Cost/package.army.UnitList.Count;


			for (int i = 0; i < availableUnit.Count; i++)
			{
				/* create unit info */
				Unit newColonel = availableUnit[i];
				Vector3 squadAvgDesiredPos = avgDesiredUnitPos[i];

				/* create squad info */
				Squad squad		= new Squad();
				int squadCost	= newColonel.Cost;

				/* remove frm list the infos */
				availableUnit.Remove(newColonel);
				avgDesiredUnitPos.Remove(squadAvgDesiredPos);

				/* add unit to squad */
				squad.Add(newColonel);

				while (squadCost < avgCost)
				{
					/* init compare variable */
					Unit addUnit	= null;
					int index		= -1;
					float tempDist	= float.MaxValue;

					/* find closest avgDesiredPos to colonel's one */
					for (int j = 0; j < availableUnit.Count; j++)
					{
						Unit jUnit = availableUnit[j];

						if ((squadAvgDesiredPos - avgDesiredUnitPos[j]).sqrMagnitude < tempDist)
						{
							tempDist = (squadAvgDesiredPos - avgDesiredUnitPos[j]).sqrMagnitude;
							addUnit = jUnit;
						}
					}

					/* add Unit if found */
					if (index > 0)
					{
						squad.Add(addUnit);

						avgDesiredUnitPos.RemoveAt(index);
						availableUnit.RemoveAt(index);

						/* when removing it might not work */
						i = 0;

						/* add cost */
						squadCost += addUnit.Cost;
					}
				}

				package.army.AddSquad(squad);
			}

            Debug.Log("army:" + package.army.SquadList.Count);
			Debug.Log("army:" + package.army.SquadList[0].Count);

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
			if (package.army.UnitList.Count > 0)
			{
				int MedCost = package.army.TotalCost() / package.army.UnitList.Count;

				if (package.army.SquadList.Count <= 0)
				{
					_priority = Seuil;
					Debug.Log(_priority);
					return;
				}

				if (package.army.CostPending() > 0)
				{
					_priority = Seuil;
					return;
				}

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
