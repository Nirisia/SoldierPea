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

	public UnitController _owner = null;

	private int _capturedTargets = 0;

	/* lists */
	private		List<Unit>		_unitList	 = new List<Unit>();
	private		List<Squad>		_squadList	 = new List<Squad>();
	protected	List<Factory>	_factoryList = new List<Factory>();

	/*====== Serialized Field Getter/Setter ======*/

	public ETeam Team => _team;

	/*====== Member Getter/Setter ======*/

	public int CapturedTargets => _capturedTargets;

	/* Add/Remove Methods */
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

		_factoryList.Add(factory);
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
