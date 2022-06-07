using System;
using UnityEngine;


public class A_Build_Data: AIActionData
{
	public override EActionType GetActionType()
	{
		return EActionType.Build;
	}

	public Army army;
	public Army enemyArmy;
	public TargetBuilding[] targetBuilding;
	public int	buildPoints;
	public Func<int, Vector3, bool> request;
}


[CreateAssetMenu(fileName = "Build", menuName = "Actions/Build")]

public class A_Build : AIAction
{
	#region Serialized Field
	/*===== Serialized Field =====*/

	[SerializeField, Min(0)] private int typeFactory = 0;
	[SerializeField] public float unitWeight = 0.5f;
	[SerializeField] public float costWeight = 0.5f;

	#endregion

	public override bool Execute(AIActionData data)
    {
        if (data is A_Build_Data package)
        {
			/* init useful data */
	        Factory factory = package.army.FactoryList[0];
	        Vector3 armyPos = package.enemyArmy.transform.position;

			/* not create factory when building unit */
	        if (factory.IsBuildingUnit)
		        return false;

			/* init variable for computing pos */
	        Vector3			pos				= Vector3.zero;
	        float			dist			= float.MaxValue;
			TargetBuilding	chosenBuilding	= null;
			bool			build			= false;

	        for (int i = 0; i < package.targetBuilding.Length; i++)
	        {
				/* get distance from the targetBuilding to the enemy army's pos */
		        Vector3 iTargetPos = package.targetBuilding[i].transform.position;
		        float iDist = (armyPos - iTargetPos).sqrMagnitude;

				/* if our target building is close enough */
		        if (iDist < dist && package.targetBuilding[i].GetTeam() == package.army.Team)
		        {
					/* change the one we create */
					chosenBuilding = package.targetBuilding[i];
					dist	= iDist;
			        pos		= iTargetPos;
					build	= true;
		        }
	        }

			if (build)
			{
				/* we do not build on the target building but near it so we compute an offset */
				Bounds targetBounds = chosenBuilding.GetComponent<Collider>().bounds;
				float offset = factory.GetBuildableFactoryData(typeFactory).RadiusOffset;

				pos	+= new Vector3(UnityEngine.Random.insideUnitCircle.x * targetBounds.extents.x * offset, 0.0f, UnityEngine.Random.insideUnitCircle.y * targetBounds.extents.z * offset);
				return package.request(typeFactory, pos);
			}

	        return build;    
        }
        else
        {
	        return false;
        }
		
    }

    public override void UpdatePriority(AIActionData data)
    {
		if (data is A_Build_Data package)
        {
			Army _army				= package.army;
			Army _enemyArmy			= package.enemyArmy;

			float totalBuildPoints	= package.buildPoints;
			float cost				= package.army.FactoryList[0].GetFactoryCost(typeFactory);
			float totalCost			= (_enemyArmy.Cost + _army.Cost);

			_priority =	Mathf.Clamp01(totalCost > 0 ? (((float)_army.Cost - _enemyArmy.Cost) /  totalCost * unitWeight) : 0.0f 
						+ ((totalBuildPoints - cost) / (cost + totalBuildPoints) * costWeight));
		}
    }
}
