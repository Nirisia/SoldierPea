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
	}

    protected override void Update()
    {
		//TODO: Update Priority (in SetTactic)

        base.Update();
        Data data = new Data();
        if(InitData(data))
            _tactician.ExecuteTactic(data);
    }

	#endregion

	#region Tactics methods
	/*===== Tactics methods =====*/

	public bool InitData(in Data data)
    {
        if (!_tactician.GetNextAction())
        {
            Debug.Log("action is null");
            return false;
        }

        //TODO: Compute value for tactics and replace hard coded
		//Do it in actions => give min data for compute.
        switch (_tactician.GetNextAction().AType)
        {
            case EActionType.MakeUnit:
                data.package.Add("Factory", _army.FactoryList[0]);
                _tactician.ChooseTypeAndCountUnit(data);
                break;
            
            case EActionType.MakeSquad:
                data.package.Add("Army", _army);
                _tactician.CreateSquad(data, _army);
                break;
            
            case EActionType.Move:
				SetMoveData(data);
				//_tactician.ChooseDestination(data, _army);
                break;
            
            case EActionType.Build:
                Func<int, Vector3, bool> request = RequestFactoryBuild;
                _selectedFactory = _army.FactoryList[0];
                data.package.Add("Factory", _army.FactoryList[0]);
                
                data.package.Add("Request", request);

                _tactician.ChooseTypeAndPosFactory(data);
                
                break;
        }

        return true;
    }

	private void SetMoveData(in Data data)
	{
		data.package.Add("OwnerArmy", _army);
		data.package.Add("EnemyArmy", _enemyArmy);
		data.package.Add("TargetBuildings", _targetBuildings);
	}

	#endregion
}
