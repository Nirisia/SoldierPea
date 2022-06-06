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
		
	    _tactician.SetTactic(InitSortData());
	}

	private List<AIActionData> InitSortData()
	{
		List<AIActionData> toReturn = new List<AIActionData>();

		toReturn.Add(SetMakeUnitData());
		toReturn.Add(Set_MakeSquad_Data());
		toReturn.Add(SetMoveData());
		toReturn.Add(SetFactoryData());

		return toReturn;
	}

	int frame = 0;

	protected override void Update()
    {
		//TODO: Update Priority (in SetTactic)


		if (frame < Time.frameCount - 10)
		{
			_tactician.SetTactic(InitSortData());
			frame = Time.frameCount;
		}

		base.Update();
		AIActionData data = InitExecData();

		_tactician.ExecuteTactic(data);
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
		List<int> typeList = new List<int>();
		typeList.Add(1);
		List<int>countsList = new List<int>();
		countsList.Add(4);

		data.TypeList = typeList;
		data.CountsList = countsList;
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
