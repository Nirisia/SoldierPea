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

	protected Army	  _army				= null;
	protected Factory _selectedFactory	= null;

	/*=============== END Members ===============*/
	#endregion

	#region Accessors
	/*=============== Accessors ===============*/

	public ETeam GetTeam() { return Team; }

	[SerializeField]
	protected int StartingBuildPoints = 15;

	/* how much seconds it takes for build points to increase by one */
	[SerializeField]
	private float IncreasBuildPointsRate = 5.0f;

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

	/* last time build point were increased */
	private float _LastIncrease = 0.0f;

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

	#region Add Unit/Factory Methods
	/*=============== Add Unit/Factory Methods ===============*/

	virtual public void AddUnit(Unit unit)
	{
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
		if (_LastIncrease < Time.time - IncreasBuildPointsRate)
		{
			_LastIncrease = Time.time;
			TotalBuildPoints++;
		}
	}
	#endregion
}
