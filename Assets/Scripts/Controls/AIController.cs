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
        Data data = new Data();
        InitData(data);
        _tactician.ExecuteTactic(data);
        

    }

    protected override void Update()
    {
        base.Update();
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
                data.package.Add("Factory", factory);
                _tactician.ChooseTypeAndCountUnit(data);
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
                _selectedFactory = factory;
                data.package.Add("Request", request);

                _tactician.ChooseTypeAndPosFactory(data);
                
                break;
        }

        return true;
    }

    #endregion
}
