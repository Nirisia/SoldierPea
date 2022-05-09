using System;
using System.Collections.Generic;
using UnityEngine;

// points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// max points can be increased by capturing TargetBuilding entities
public class UnitController : MonoBehaviour
{
	#region Serialized Fields
	/*=============== Serialized Fields ===============*/

	[SerializeField]
    protected ETeam Team = ETeam.Neutral;

	/*=============== END Serialized Fields ===============*/
	#endregion

	#region Members
	/*=============== Members ===============*/

	protected Army					_army			 = new Army();
	protected SelectableList<Unit>	_selectedUnits	 = new SelectableList<Unit>();
	protected Factory				_selectedFactory = null;

	/*=============== END Members ===============*/
	#endregion

	#region Accessors
	/*=============== Accessors ===============*/

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

    public Transform GetTeamRoot() { return _army.transform; }

	/*=============== Accessors ===============*/
	#endregion

	#region Events
	/*=============== Events ===============*/

	protected Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

	/*=============== END Events ===============*/
	#endregion

	#region Unit Selection methods
	/*=============== Unit Selection Methods ===============*/

	protected void UnselectAllUnits()
    {
        _selectedUnits.Clear();
    }

    protected void SelectAllUnits()
    {
        _selectedUnits.Clear();
        _selectedUnits.AddRange(_army.UnitList);
    }

    protected void SelectAllUnitsByTypeId(int typeId)
    {
        UnselectCurrentFactory();
        UnselectAllUnits();
        _selectedUnits = _army.UnitList.FindAll(delegate (Unit unit)
            {
                return unit.GetTypeId == typeId;
            }
        );
    }

    protected void SelectUnitList(List<Unit> units)
    {
        _selectedUnits.AddRange(units);
    }

    protected void SelectUnitList(Unit [] units)
    {
        _selectedUnits.AddRange(units);
    }

    protected void SelectUnit(Unit unit)
    {
        _selectedUnits.Add(unit);
    }

    protected void UnselectUnit(Unit unit)
    {
        _selectedUnits.Remove(unit);
    }

	/*=============== END Selection Methods ===============*/
	#endregion

	#region Add Unit/Factory Methods
	/*=============== Add Unit/Factory Methods ===============*/

	virtual public void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            TotalBuildPoints += unit.Cost;
            if (unit.IsSelected)
                _selectedUnits.Remove(unit);
        };
        _army.AddUnit(unit);
    }

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
				_selectedFactory = null;
        };

		_army.AddFactory(factory);
    }

	/*=============== END Add Unit/Factory Methods ===============*/
	#endregion

	#region Target methods
	/*=============== Target Methods ===============*/

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

	/*=============== END Target Methods ===============*/
	#endregion

	#region Factory Selection methods
	/*=============== Factory Selection Methods ===============*/

	virtual protected void SelectFactory(Factory factory)
    {
        if (factory == null || factory.IsUnderConstruction)
            return;

        _selectedFactory = factory;
		_selectedFactory.SetSelected(true);
        UnselectAllUnits();
    }

    virtual protected void UnselectCurrentFactory()
    {
        if (_selectedFactory != null)
			_selectedFactory.SetSelected(false);
		_selectedFactory = null;
    }

	/*=============== Factory Selection Methods ===============*/
	#endregion

	#region Unit/Factory Build methods
	/*=============== Unit/Factory Build Methods ===============*/

	protected bool RequestUnitBuild(int unitMenuIndex)
    {
        if (_selectedFactory == null)
            return false;

        return _selectedFactory.RequestUnitBuild(unitMenuIndex);
    }

    protected bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos)
    {
        if (_selectedFactory == null)
            return false;

        int cost = _selectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (_selectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        Factory newFactory = _selectedFactory.StartBuildFactory(factoryIndex, buildPos);
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
				_army = armies[i];
				break;
			}

		_army._owner = this;
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
