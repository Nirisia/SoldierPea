using System;
using System.Collections.Generic;
using UnityEngine;

// points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// max points can be increased by capturing TargetBuilding entities
public class UnitController : MonoBehaviour
{
    [SerializeField]
    protected ETeam Team;

	[SerializeField]
	private Army army = new Army();

    public ETeam GetTeam() { return Team; }

    [SerializeField]
    protected int StartingBuildPoints = 15;

    protected int _TotalBuildPoints = 0;
    public int TotalBuildPoints
    {
        get { return _TotalBuildPoints; }
        set
        {
            Debug.Log("TotalBuildPoints updated");
            _TotalBuildPoints = value;
            OnBuildPointsUpdated?.Invoke();
        }
    }

    protected int _CapturedTargets = 0;
    public int CapturedTargets
    {
        get { return _CapturedTargets; }
        set
        {
            _CapturedTargets = value;
            OnCaptureTarget?.Invoke();
        }
    }

    public Transform GetTeamRoot() { return army.transform; }

    protected List<Unit> SelectedUnitList = new List<Unit>();
    protected Factory SelectedFactory = null;

    // events
    protected Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

    #region Unit methods
    protected void UnselectAllUnits()
    {
        foreach (Unit unit in SelectedUnitList)
            unit.SetSelected(false);
        SelectedUnitList.Clear();
    }
    protected void SelectAllUnits()
    {
        foreach (Unit unit in UnitList)
            unit.SetSelected(true);

        SelectedUnitList.Clear();
        SelectedUnitList.AddRange(UnitList);
    }
    protected void SelectAllUnitsByTypeId(int typeId)
    {
        UnselectCurrentFactory();
        UnselectAllUnits();
        SelectedUnitList = UnitList.FindAll(delegate (Unit unit)
            {
                return unit.GetTypeId == typeId;
            }
        );
        foreach (Unit unit in SelectedUnitList)
        {
            unit.SetSelected(true);
        }
    }
    protected void SelectUnitList(List<Unit> units)
    {
        foreach (Unit unit in units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(units);
    }
    protected void SelectUnitList(Unit [] units)
    {
        foreach (Unit unit in units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(units);
    }
    protected void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);
        SelectedUnitList.Add(unit);
    }
    protected void UnselectUnit(Unit unit)
    {
        unit.SetSelected(false);
        SelectedUnitList.Remove(unit);
    }
    virtual public void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            TotalBuildPoints += unit.Cost;
            if (unit.IsSelected)
                SelectedUnitList.Remove(unit);
            UnitList.Remove(unit);
        };
        UnitList.Add(unit);
    }
    public void CaptureTarget(int points)
    {
        Debug.Log("CaptureTarget");
        TotalBuildPoints += points;
        CapturedTargets++;
    }
    public void LoseTarget(int points)
    {
        TotalBuildPoints -= points;
        CapturedTargets--;
    }
    #endregion

    #region Factory methods
    void AddFactory(Factory factory)
    {
        if (factory == null)
        {
            Debug.LogWarning("Trying to add null factory");
            return;
        }

        factory.OnDeadEvent += () =>
        {
            TotalBuildPoints += factory.Cost;
            if (factory.IsSelected)
                SelectedFactory = null;
            FactoryList.Remove(factory);
        };
        FactoryList.Add(factory);
    }
    virtual protected void SelectFactory(Factory factory)
    {
        if (factory == null || factory.IsUnderConstruction)
            return;

        SelectedFactory = factory;
        SelectedFactory.SetSelected(true);
        UnselectAllUnits();
    }
    virtual protected void UnselectCurrentFactory()
    {
        if (SelectedFactory != null)
            SelectedFactory.SetSelected(false);
        SelectedFactory = null;
    }
    protected bool RequestUnitBuild(int unitMenuIndex)
    {
        if (SelectedFactory == null)
            return false;

        return SelectedFactory.RequestUnitBuild(unitMenuIndex);
    }
    protected bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos)
    {
        if (SelectedFactory == null)
            return false;

        int cost = SelectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (SelectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        Factory newFactory = SelectedFactory.StartBuildFactory(factoryIndex, buildPos);
        if (newFactory != null)
        {
            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            return true;
        }
        return false;
    }
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
		Army[] armies = FindObjectsOfType<Army>();

		for (int i = 0; i < armies.Length; i++)
			if (armies[i].Team == Team)
			{
				army = armies[i];
				break;
			}

		army._owner = this;
    }

    virtual protected void Start ()
    {
        TotalBuildPoints = StartingBuildPoints;

        


    }
    virtual protected void Update ()
    {
	}
    #endregion
}
