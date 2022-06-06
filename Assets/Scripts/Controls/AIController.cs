using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


// $$$ TO DO :)


public sealed class AIController : UnitController
{
	#region Members 
	/*===== Members =====*/

	public AITactician			_tactician;
	private Army				_enemyArmy		 = null;
	private TargetBuilding[]	_targetBuildings = null;

	#endregion

	#region MonoBehaviour methods
	/*===== MonoBehaviour methods =====*/

	protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

		Army[] armies = FindObjectsOfType<Army>();

		/* there is only two possible teams */
		for (int i = 0; i < armies.Length; i++)
			if (armies[i].Team != Team)
			{
				_enemyArmy = armies[i];
				break;
			}

		_targetBuildings = GameServices.GetTargetBuildings();

		//AIActionData data = InitSortData();
		
	    _tactician.SetTactic(InitExecData());
	}

	/*private AIActionData InitSortData()
	{
		AIActionData toReturn = new AIActionData();

		toReturn.package.Add("Army", _army);
		toReturn.package.Add("EnemyArmy", _enemyArmy);
		toReturn.package.Add("TargetBuildings", _targetBuildings);
		toReturn.package.Add("OwnerBuildPoints", _TotalBuildPoints);


		return toReturn;
	}*/

    protected override void Update()
    {
		//TODO: Update Priority (in SetTactic)

        base.Update();
	    _tactician.ExecuteTactic(InitExecData());
    }

	#endregion

	#region Tactics methods
	/*===== Tactics methods =====*/

	public AIActionData InitExecData()
    {
        if (!_tactician.GetNextAction())
        {
            Debug.Log("action is null");
            return null;
        }

        //TODO: Compute value for tactics and replace hard coded
		//Do it in actions => give min data for compute.
        switch (_tactician.GetNextAction().AType)
        {
            case EActionType.MakeUnit:
				return SetMakeUnitData();
            
            case EActionType.MakeSquad:
	            return Set_MakeSquad_Data();
            
            case EActionType.Move:
				return SetMoveData();

            case EActionType.Build:
	            return SetFactoryData();
        }

        return null;
    }

	private A_MakeUnit_Data SetMakeUnitData()
	{

		A_MakeUnit_Data data = new A_MakeUnit_Data();
		
		data.army = _army;
		data.enemyArmy = _enemyArmy;
		data.buildPoints = _TotalBuildPoints;

		return data;
	}

	private A_Move_Data SetMoveData()
	{
		A_Move_Data data = new A_Move_Data();
		
		data.enemyArmy = _enemyArmy;
		data.myArmy = _army;
		data.targetBuilding = _targetBuildings;
		data.myBuildPoint = _TotalBuildPoints;

		return data;
	}
	
	private A_Squad_Data Set_MakeSquad_Data()
	{
		A_Squad_Data data = new A_Squad_Data();
		data.army = _army;
		data.targets = _targetBuildings;
		data.EnemyArmy = _enemyArmy;
		
		return data;
	}
	
	private A_Build_Data SetFactoryData()
	{
		A_Build_Data data = new A_Build_Data();
		
		data.enemyArmy = _enemyArmy;
		data.army = _army;
		data.targetBuilding = _targetBuildings;
		data.buildPoints = _TotalBuildPoints;
		
		Func<int, Vector3, bool> request = RequestFactoryBuild;
		data.request = request;
		
		return data;
	}

	#endregion
}
