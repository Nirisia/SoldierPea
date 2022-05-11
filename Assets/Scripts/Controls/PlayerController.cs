using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public sealed class PlayerController : UnitController
{
	public enum InputMode
	{
		Orders,
		FactoryPositioning
	}

	#region Serialized Fields 
	/*===== Serialized Fields =====*/

	[SerializeField]
	GameObject TargetCursorPrefab = null;
	[SerializeField]
	float TargetCursorFloorOffset = 0.2f;
	[SerializeField]
	EventSystem SceneEventSystem = null;

	[SerializeField, Range(0f, 1f)]
	float FactoryPreviewTransparency = 0.3f;

	/*===== END Serialized Fields =====*/
	#endregion

	#region Members 
	/*===== Members =====*/

	/* Build Menu UI */
	MenuController PlayerMenuController;

	/* Camera */
	TopCamera	TopCameraRef		= null;
	bool		CanMoveCamera		= false;
	Vector2		CameraInputPos		= Vector2.zero;
	Vector2		CameraPrevInputPos	= Vector2.zero;
	Vector2		CameraFrameMove		= Vector2.zero;

	/* Selection */
	Vector3			SelectionStart			= Vector3.zero;
	Vector3			SelectionEnd			= Vector3.zero;
	bool			SelectionStarted		= false;
	float			SelectionBoxHeight		= 50f;
	LineRenderer	SelectionLineRenderer;
	GameObject		TargetCursor			= null;

	private SelectableList<Unit>	_selectedUnits		= new SelectableList<Unit>();

	/* Factory build */
	InputMode	CurrentInputMode		= InputMode.Orders;
	int			WantedFactoryId			= 0;
	GameObject	WantedFactoryPreview	= null;
	Shader		PreviewShader			= null;

	/*===== END Members =====*/
	#endregion

	#region Events 
	/*===== Events =====*/

	/* UI event */
	PointerEventData MenuPointerEventData = null;

	/* Unit Action events */
	Action OnUnitActionStart		= null;
	Action OnUnitActionEnd			= null;

	/* factory event */
	Action<Vector3> OnFactoryPositioned		= null;

	/* Keyboard events */
	Action OnDestroyEntityPressed		= null;
	Action [] OnCategoryPressed			= new Action[9];

	/*===== END Events =====*/
	#endregion

	#region Getter/Setter 
	/*===== Getter/Setter =====*/

	GameObject GetTargetCursor()
	{
		if (TargetCursor == null)
		{
			TargetCursor		= Instantiate(TargetCursorPrefab);
			TargetCursor.name	= TargetCursor.name.Replace("(Clone)", "");
		}
		return TargetCursor;
	}

	void SetTargetCursorPosition(Vector3 pos)
	{
		SetTargetCursorVisible(true);
		pos.y += TargetCursorFloorOffset;
		GetTargetCursor().transform.position = pos;
	}

	void SetTargetCursorVisible(bool isVisible)
	{
		GetTargetCursor().SetActive(isVisible);
	}

	void SetCameraFocusOnMainFactory()
	{
		if (_army.FactoryList.Count > 0)
			TopCameraRef.FocusEntity(_army.FactoryList[0]);
	}

	void CancelCurrentBuild()
	{
		_selectedFactory?.CancelCurrentBuild();
		PlayerMenuController.HideAllFactoryBuildQueue();
	}

	/*===== END Getter/Setter =====*/
	#endregion

	#region MonoBehaviour methods
	/*===== MonoBehaviour Methods =====*/

	protected override void Awake()
	{
		base.Awake();

		/* get UI */
		PlayerMenuController = GetComponent<MenuController>();
		if (PlayerMenuController == null)
			Debug.LogWarning("could not find MenuController component !");

		/* set UI events */
		OnBuildPointsUpdated += PlayerMenuController.UpdateBuildPointsUI;
		OnCaptureTarget		 += PlayerMenuController.UpdateCapturedTargetsUI;

		/* get camera */
		TopCameraRef			= Camera.main.GetComponent<TopCamera>();

		/* get line renderer for preview of selection */
		SelectionLineRenderer	= GetComponent<LineRenderer>();
	   
		if (SceneEventSystem == null)
		{
			Debug.LogWarning("EventSystem not assigned in PlayerController, searching in current scene...");
			SceneEventSystem = FindObjectOfType<EventSystem>();
		}

		/* Set up the new Pointer Event */
		MenuPointerEventData = new PointerEventData(SceneEventSystem);

		Squad selectSquad = new Squad();
		_army.AddSquad(selectSquad);
	}

	override protected void Start()
	{
		base.Start();

		/* get selection quad shader */
		PreviewShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

		/* right click : Unit actions (move / attack / capture ...) */
		OnUnitActionEnd += ComputeUnitsAction;

		/* set factory event */
		OnFactoryPositioned += (floorPos) =>
		{
			if (RequestFactoryBuild(WantedFactoryId, floorPos))
			{
				ExitFactoryBuildMode();
			}
		};

		/* Destroy selected unit event */
		OnDestroyEntityPressed += () =>
		{
			Unit[] unitsToBeDestroyed = _selectedUnits.ToArray();
			foreach (Unit unit in unitsToBeDestroyed)
			{
				(unit as IDamageable).Destroy();
			}

			if (_selectedFactory)
			{
				Factory factoryRef = _selectedFactory;
				UnselectCurrentFactory();
				factoryRef.Destroy();
			}
		};

		/* create Select unit Type events */
		for(int i = 0; i < OnCategoryPressed.Length; i++)
		{
			// store typeId value for event closure
			int typeId = i;
			OnCategoryPressed[i] += () =>
			{
				SelectAllUnitsByTypeId(typeId);
			};
		}
	}

	override protected void Update()
	{
		/* gameplay Update (selection/position/action...) */
		switch (CurrentInputMode)
		{
			case InputMode.FactoryPositioning:
				UpdateFactoryPositioningInput();
				break;
			case InputMode.Orders:
				UpdateSelectionInput();
				UpdateActionInput();
				break;
		}

		/* Camera update: get input, then apply */
		UpdateCameraInput();
		UpdateMoveCamera();
	}

	/*===== END MonoBehaviour Methods =====*/
	#endregion

	#region Add Unit Method
	/*=============== Add Unit Method ===============*/

	override public void AddUnit(Unit unit)
	{
		unit.OnDeadEvent += () =>
		{
			TotalBuildPoints += unit.Cost;
			if (unit.IsSelected)
				_selectedUnits.Remove(unit);
		};

		base.AddUnit(unit);
	}

	/*=============== END Add Unit Method ===============*/
	#endregion

	#region Unit Selection methods
	/*=============== Unit Selection Methods ===============*/

	private void UnselectAllUnits()
	{
		_selectedUnits.Clear();
		_army.SquadList[0]._group = _selectedUnits.List;
	}

	private void SelectAllUnits()
	{
		_selectedUnits.Clear();
		_selectedUnits.AddRange(_army.UnitList);
	}

	private void SelectAllUnitsByTypeId(int typeId)
	{
		UnselectCurrentFactory();
		UnselectAllUnits();
		_selectedUnits = _army.UnitList.FindAll(delegate (Unit unit)
		{
			return unit.GetTypeId == typeId;
		}
		);
	}

	private void SelectUnitList(List<Unit> units)
	{
		_selectedUnits.AddRange(units);
	}

	private void SelectUnitList(Unit[] units)
	{
		_selectedUnits.AddRange(units);
	}

	private void SelectUnit(Unit unit)
	{
		_selectedUnits.Add(unit);
	}

	private void UnselectUnit(Unit unit)
	{
		_selectedUnits.Remove(unit);
		_army.SquadList[0]._group = _selectedUnits.List;
	}
	/*=============== END Selection Methods ===============*/
	#endregion

	#region Update methods
	/*===== Update methods =====*/

	/* method that positions a factory on left button down 
	 * or exits Factory build mode on escape */
	void UpdateFactoryPositioningInput()
	{
		Vector3 floorPos = ProjectFactoryPreviewOnFloor();

		if (Input.GetKeyDown(KeyCode.Escape))
			ExitFactoryBuildMode();
		if (Input.GetMouseButtonDown(0))
			OnFactoryPositioned?.Invoke(floorPos);
	}

	/* method that listens for all selection inputs of user */
	void UpdateSelectionInput()
	{
		/* Select All */
		if (Input.GetKeyDown(KeyCode.A))
			SelectAllUnits();

		/* Select by Type */
		for (int i = 0; i < OnCategoryPressed.Length; i++)
		{
			if (Input.GetKeyDown(KeyCode.Keypad1 + i) || Input.GetKeyDown(KeyCode.Alpha1 + i))
			{
				OnCategoryPressed[i]?.Invoke();
				break;
			}
		}

		// Update mouse inputs
#if UNITY_EDITOR
		if (EditorWindow.focusedWindow != EditorWindow.mouseOverWindow)
			return;
#endif
		/* left mouse button, selection input by zone */
		if (Input.GetMouseButtonDown(0))//down
			StartSelection();
		else if (Input.GetMouseButton(0))//hold
			UpdateSelection();
		else if (Input.GetMouseButtonUp(0))//up
			EndSelection();

	}

	/* method that listens for all actions inputs of user */
	void UpdateActionInput()
	{
		/* destroy selected units */
		if (Input.GetKeyDown(KeyCode.Delete))
			OnDestroyEntityPressed?.Invoke();

		/* cancel build */
		if (Input.GetKeyDown(KeyCode.C))
			CancelCurrentBuild();

		/* Contextual unit actions (attack / capture ...) */
		if (Input.GetMouseButtonDown(1))//right mouse button down
			OnUnitActionStart?.Invoke();
		if (Input.GetMouseButtonUp(1))//right mouse button up
			OnUnitActionEnd?.Invoke();
	}

	/* method that listens for all inputs of camera control of user */
	void UpdateCameraInput()
	{
		/* Camera focus */
		if (Input.GetKeyDown(KeyCode.F))
			SetCameraFocusOnMainFactory();

		/* Camera movement inputs */

		/* keyboard move (arrows) */
		float hValue = Input.GetAxis("Horizontal");
		if (hValue != 0)
			TopCameraRef.KeyboardMoveHorizontal(hValue);
		float vValue = Input.GetAxis("Vertical");
		if (vValue != 0)
			TopCameraRef.KeyboardMoveVertical(vValue);

		/* zoom in / out (ScrollWheel) */
		float scrollValue = Input.GetAxis("Mouse ScrollWheel");
		if (scrollValue != 0)
			TopCameraRef.Zoom(scrollValue);

		/* drag move (mouse button) */
		if (Input.GetMouseButtonDown(2))//middle mouse button down
			StartMoveCamera();
		if (Input.GetMouseButtonUp(2))//middle mouse button up
			StopMoveCamera();
	}

	/*===== End Update methods =====*/
	#endregion

	#region Selection methods
	/*===== Selection methods =====*/

	void TrySelectFactory(Factory factory_)
	{
		if (factory_ != null)
		{
			if (factory_.GetTeam() == Team && _selectedFactory != factory_)
			{
				UnselectCurrentFactory();
				SelectFactory(factory_);
			}
		}
	}

	void TrySelectUnit(Unit unit_)
	{
		bool isShiftBtPressed = Input.GetKey(KeyCode.LeftShift);
		bool isCtrlBtPressed = Input.GetKey(KeyCode.LeftControl);

		UnselectCurrentFactory();

		if (unit_ != null && unit_.GetTeam() == Team)
		{
			if (isShiftBtPressed)
				UnselectUnit(unit_);
			else if (isCtrlBtPressed)
				SelectUnit(unit_);
			else
			{
				UnselectAllUnits();
				SelectUnit(unit_);
			}
		}
	}

	void StartSelection()
	{
		// Hide target cursor
		SetTargetCursorVisible(false);

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		int factoryMask = 1 << LayerMask.NameToLayer("Factory");
		int unitMask	= 1 << LayerMask.NameToLayer("Unit");
		int floorMask	= 1 << LayerMask.NameToLayer("Floor");

		/* Ignores Unit selection when clicking on UI.
		 * Set the Pointer Event Position to that of the mouse position */
		MenuPointerEventData.position = Input.mousePosition;

		/* Create a list of Raycast Results */
		List<RaycastResult> results = new List<RaycastResult>();
		PlayerMenuController.BuildMenuRaycaster.Raycast(MenuPointerEventData, results);
		if (results.Count > 0)//UI being touched, so get out
			return;

		RaycastHit raycastInfo;
		/* factory selection */
		if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, factoryMask))
		{
			Factory factory = raycastInfo.transform.GetComponent<Factory>();
			TrySelectFactory(factory);
		}
		/* unit selection / unselection */
		else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, unitMask))
		{
			Unit selectedUnit = raycastInfo.transform.GetComponent<Unit>();
			TrySelectUnit(selectedUnit);

		}
		else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
		{
			UnselectCurrentFactory();
			SelectionLineRenderer.enabled = true;

			SelectionStarted = true;

			SelectionStart.x = raycastInfo.point.x;
			SelectionStart.y = 0.0f;//raycastInfo.point.y + 1f;
			SelectionStart.z = raycastInfo.point.z;
		}
	}

	/*
	 * Multi selection methods
	 */
	void UpdateSelection()
	{
		if (SelectionStarted == false)
			return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		int floorMask = 1 << LayerMask.NameToLayer("Floor");

		RaycastHit raycastInfo;
		if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
		{
			SelectionEnd = raycastInfo.point;
		}

		SelectionLineRenderer.SetPosition(0, new Vector3(SelectionStart.x, SelectionStart.y, SelectionStart.z));
		SelectionLineRenderer.SetPosition(1, new Vector3(SelectionStart.x, SelectionStart.y, SelectionEnd.z));
		SelectionLineRenderer.SetPosition(2, new Vector3(SelectionEnd.x, SelectionStart.y, SelectionEnd.z));
		SelectionLineRenderer.SetPosition(3, new Vector3(SelectionEnd.x, SelectionStart.y, SelectionStart.z));
	}

	void EndSelection()
	{
		if (SelectionStarted == false)
			return;

		UpdateSelection();
		SelectionLineRenderer.enabled = false;
		Vector3 center	= (SelectionStart + SelectionEnd) / 2f;
		Vector3 size	= Vector3.up * SelectionBoxHeight + SelectionEnd - SelectionStart;
		size.x = Mathf.Abs(size.x);
		size.y = Mathf.Abs(size.y);
		size.z = Mathf.Abs(size.z);

		UnselectAllUnits();
		UnselectCurrentFactory();

		int unitLayerMask		= 1 << LayerMask.NameToLayer("Unit");
		int factoryLayerMask	= 1 << LayerMask.NameToLayer("Factory");
		Collider[] colliders = Physics.OverlapBox(center, size / 2f, Quaternion.identity, unitLayerMask | factoryLayerMask, QueryTriggerInteraction.Ignore);
		foreach (Collider col in colliders)
		{
			ISelectable selectedEntity = col.transform.GetComponent<ISelectable>();
			if (selectedEntity.GetTeam() == GetTeam())
			{
				if (selectedEntity is Unit)
					SelectUnit((selectedEntity as Unit));
				else if (selectedEntity is Factory)
					if (_selectedFactory == null)// can select only one factory at a time
						SelectFactory(selectedEntity as Factory);
			}
		}

		SelectionStarted	= false;
		SelectionStart		= Vector3.zero;
		SelectionEnd		= Vector3.zero;
	}

	/*===== END Unit selection methods =====*/
	#endregion

	#region Factory / build methods
	/*===== Factory / build methods =====*/

	public void UpdateFactoryBuildQueueUI(int entityIndex)
	{
		PlayerMenuController.UpdateFactoryBuildQueueUI(entityIndex, _selectedFactory);
	}

	protected override void SelectFactory(Factory factory)
	{
		if (factory == null || factory.IsUnderConstruction)
			return;

		base.SelectFactory(factory);
		UnselectAllUnits();

		PlayerMenuController.UpdateFactoryMenu(_selectedFactory, RequestUnitBuild, EnterFactoryBuildMode);
	}

	protected override void UnselectCurrentFactory()
	{
		if (_selectedFactory)
		{
			PlayerMenuController.UnregisterBuildButtons(_selectedFactory.AvailableUnitsCount, _selectedFactory.AvailableFactoriesCount);
		}

		PlayerMenuController.HideFactoryMenu();

		base.UnselectCurrentFactory();
	}

	void EnterFactoryBuildMode(int factoryId)
	{
		if (_selectedFactory.GetFactoryCost(factoryId) > TotalBuildPoints)
			return;

		CurrentInputMode = InputMode.FactoryPositioning;

		WantedFactoryId = factoryId;

		// Create factory preview

		// Load factory prefab for preview
		GameObject factoryPrefab = _selectedFactory.GetFactoryPrefab(factoryId);
		if (factoryPrefab == null)
		{
			Debug.LogWarning("Invalid factory prefab for factoryId " + factoryId);
		}
		WantedFactoryPreview		= Instantiate(factoryPrefab.transform.GetChild(0).gameObject); // Quick and dirty access to mesh GameObject
		WantedFactoryPreview.name	= WantedFactoryPreview.name.Replace("(Clone)", "_Preview");
		// Set transparency on materials
		foreach(Renderer rend in WantedFactoryPreview.GetComponentsInChildren<MeshRenderer>())
		{
			Material mat	= rend.material;
			mat.shader		= PreviewShader;
			Color col		= mat.color;
			col.a			= FactoryPreviewTransparency;
			mat.color		= col;
		}

		// Project mouse position on ground to position factory preview
		ProjectFactoryPreviewOnFloor();
	}
	void ExitFactoryBuildMode()
	{
		CurrentInputMode = InputMode.Orders;
		Destroy(WantedFactoryPreview);
	}

	Vector3 ProjectFactoryPreviewOnFloor()
	{
		if (CurrentInputMode == InputMode.Orders)
		{
			Debug.LogWarning("Wrong call to ProjectFactoryPreviewOnFloor : CurrentInputMode = " + CurrentInputMode.ToString());
			return Vector3.zero;
		}

		Vector3 floorPos	= Vector3.zero;
		Ray ray				= Camera.main.ScreenPointToRay(Input.mousePosition);
		int floorMask		= 1 << LayerMask.NameToLayer("Floor");
		RaycastHit raycastInfo;
		if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
		{
			floorPos = raycastInfo.point;
			WantedFactoryPreview.transform.position = floorPos;
		}
		return floorPos;
	}

	/*===== END Factory / build methods =====*/
	#endregion

	#region Entity movement method
	void ComputeUnitsAction()
	{
		if (_selectedUnits.Count == 0)
			return;

		int floorMask		= 1 << LayerMask.NameToLayer("Floor");
		Ray ray				= Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit raycastInfo;
		
		/* Set unit move target */
		if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
		{
			Vector3 newPos = raycastInfo.point;
			SetTargetCursorPosition(newPos);

			// Direct call to moving task $$$ to be improved by AI behaviour
			_army.SquadList[0]._group = _selectedUnits.List;
			_army.SquadList[0].Move(newPos);
		}
	}
	#endregion

	#region Camera methods
	/*===== Camera methods =====*/
	void StartMoveCamera()
	{
		CanMoveCamera		= true;
		CameraInputPos		= new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		CameraPrevInputPos	= CameraInputPos;
	}

	void StopMoveCamera()
	{
		CanMoveCamera = false;
	}

	void UpdateMoveCamera()
	{
		if (CanMoveCamera)
		{
			CameraInputPos	= new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			CameraFrameMove = CameraPrevInputPos - CameraInputPos;
			TopCameraRef.MouseMove(CameraFrameMove);
			CameraPrevInputPos = CameraInputPos;
		}
	}
	/*===== END Camera methods =====*/
	#endregion
}
