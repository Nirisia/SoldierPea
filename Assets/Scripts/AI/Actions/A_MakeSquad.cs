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


			for (int i = 0; i < availableUnit.Count; i++)
			{
				/* create unit info */
				Unit newColonel = availableUnit[i];

				/* create squad info */
				Squad squad		= new Squad();

				/* remove frm list the infos */
				availableUnit.Remove(newColonel);

				/* add unit to squad */
				squad.Add(newColonel);


					/* find closest avgDesiredPos to colonel's one */
				for (int j = 0; j < availableUnit.Count; j++)
				{
					Unit jUnit = availableUnit[j];

					if ((jUnit.transform.position - newColonel.transform.position).sqrMagnitude < Radius * Radius)
					{
						squad.Add(jUnit);
						availableUnit.RemoveAt(j);

						/* when removing, start all over*/
						i = -1;
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
				for (int i = 0; i < package.army.UnitList.Count; i++)
				{
					if (package.army.UnitList[i].OnChangeSquadEvent == null)
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
