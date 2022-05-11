using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Army : MonoBehaviour
{
	/*====== Serialized Field ======*/

	[SerializeField]
	private ETeam _team;

	/* squad boids settings */
	[SerializeField]private float _squadAlignement	= 0.5f;
	[SerializeField]private float _squadCohesion	= 0.5f;
	[SerializeField]private float _squadSeparation	= 0.5f;

	/*====== Members ======*/

	public UnitController _owner = null;

	private int _capturedTargets = 0;

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
		};
		_unitList.Add(unit);
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
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
