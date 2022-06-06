using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Army : MonoBehaviour
{
	/*====== Serialized Field ======*/

	[SerializeField]
	private ETeam _team;

	/*====== Members ======*/

	[HideInInspector] public UnitController _owner = null;

	private int _capturedTargets = 0;

	private int _cost = 0;

	/* lists */
	private	List<Unit>		_unitList	 = new List<Unit>();
	private	List<Squad>		_squadList	 = new List<Squad>();
	private	List<Factory>	_factoryList = new List<Factory>();

	/*====== Serialized Field Getter/Setter ======*/

	public ETeam Team => _team;

	/*====== Member Getter/Setter ======*/

	public int CapturedTargets => _capturedTargets;

	public List<Unit> UnitList		=> _unitList;
	public List<Factory> FactoryList => _factoryList;
	public List<Squad> SquadList => _squadList;

	public int Cost => _cost;

	private void OnDrawGizmos()
	{
		if (_squadList.Count > 0)
			Gizmos.DrawSphere(_squadList[0].Position, 1.0f);
	}


	/* Add/Remove Methods */
	public void AddFactory(Factory factory)
	{
		if (factory == null)
		{
			Debug.LogWarning("Trying to add null factory");
			return;
		}

		factory.OnDeadEvent += () =>
		{
			_owner.TotalBuildPoints += factory.Cost;
			_factoryList.Remove(factory);
		};

		_factoryList.Add(factory);
	}

	virtual public void AddUnit(Unit unit)
	{
		unit.OnDeadEvent += () =>
		{
			_unitList.Remove(unit);
			_cost -= unit.Cost;
		};
		_unitList.Add(unit);
		_cost += unit.Cost;
	}

	virtual public void AddSquad(Squad squad)
	{
		_squadList.Add(squad);
	}
	
	/*====== Init Methods ======*/

	void GetTeamExistingFactory()
	{
		// get all team factory already in scene
		Factory[] allFactories = FindObjectsOfType<Factory>();
		foreach (Factory factory in allFactories)
		{
			if (factory.GetTeam() == _team)
			{
				AddFactory(factory);
			}
		}

		Debug.Log("found " + _factoryList.Count + " factory for team " + _team.ToString());
	}

	// Start is called before the first frame update
	void Awake()
	{
		GetTeamExistingFactory();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _squadList.Count; i++)
		{
			if (_squadList[i].Count <= 0)
			{
				_squadList.Remove(_squadList[i--]);
				continue;
			}

			_squadList[i].UpdateMovement();
		}
	}
}
