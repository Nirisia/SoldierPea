using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Actions/Move")]

public class A_Move : AIAction
{

	struct A_Move_Data
	{
		public Army				myArmy;
		public Army				enemyArmy;
		public TargetBuilding[]	targetBuilding;
	}

    public override bool Execute(Data data)
    {
		A_Move_Data moveData;
		UnpackMoveData(out moveData, data);


		
        
        for (int i = 0; i < squads.Count; i++)
        {
            squads[i].Move(pos[i]);
        }
        
        return true;
    }

    public override void UpdatePriority(Data data)
    {
        
    }

	private bool UnpackMoveData(out A_Move_Data moveData_, Data abstractData_)
	{
		moveData_ = new A_Move_Data();
		if (abstractData_.package.Count != 3)
		{
			Debug.Log("A_Move: bad package size");
			return false;
		}

		foreach (var pack in abstractData_.package)
		{
			switch (pack.Key)
			{
				case "OwnerArmy":
					moveData_.myArmy	= (Army)pack.Value;
					break;

				case "EnemyArmy":
					moveData_.enemyArmy = (Army)pack.Value;
					break;

				case "TargetBuildings":
					moveData_.targetBuilding = (TargetBuilding[])pack.Value;
					break;
				default:
					Debug.LogWarning("bad package");
					return false;
			}
		}
	}

}
