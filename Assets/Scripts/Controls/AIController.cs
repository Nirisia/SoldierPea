using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


// $$$ TO DO :)


public sealed class AIController : UnitController
{
    
    public Factory factory;
    public AITactician _tactician;
    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        _army.AddFactory(factory);
        

    }

    protected override void Update()
    {
        base.Update();
        Data data = new Data();
        if(InitData(data))
            _tactician.ExecuteTactic(data);
    }
    
    public bool InitData(in Data data)
    {
        if (!_tactician.GetNextAction())
        {
            Debug.Log("action is null");
            return false;
        }

        
        switch (_tactician.GetNextAction().AType)
        {
            case EActionType.MakeUnit:
                Army[] armies = FindObjectsOfType<Army>();
                for (int i = 0; i < armies.Length; i++)
                {
                    if (armies[i].Team != Team)
                    {
                        data.package.Add("EnemyArmy", armies[i]);
                        break;
                    }
                }    
                data.package.Add("Army", _army);
                data.package.Add("BuildPoints", TotalBuildPoints);
                
               // _tactician.ChooseTypeAndCountUnit(data);
                break;
            
            case EActionType.MakeSquad:
                data.package.Add("Army", _army);
                _tactician.CreateSquad(data, _army);
                break;
            
            case EActionType.Move:
                _tactician.ChooseDestination(data, _army);
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

    #endregion
}
