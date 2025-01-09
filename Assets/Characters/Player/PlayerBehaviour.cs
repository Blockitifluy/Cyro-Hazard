using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerBehaviour : CharacterControl
{
	private InputActionMap _InputActionMap;
	private InputAction _MovementAction;
	private GameObject _CameraObject;
	private Camera _Camera;

	public InputActionAsset Controls;

	[Header("Pickup")]
	[InspectorName("Character Pickup Distance")]
	public float CharacterPickupDist = 5;
	[InspectorName("Camera Pickup Distance")]
	public float MousePickupDist = 2.5f;

	public GameObject[] GetSelectableDrops()
	{
		List<GameObject> res = new();

		RaycastHit? nullableMouseHit = Helper.GetMouseRayHitInfo(_Camera);

		if (!nullableMouseHit.HasValue)
			return new GameObject[0];
		var mouseHit = nullableMouseHit.GetValueOrDefault();

		var drops = GameObject.FindGameObjectsWithTag("Dropped Items");
		foreach (GameObject drp in drops)
		{
			// Distance from character model
			// Distance from mouse

			float charDist = (drp.transform.position - transform.position).magnitude;
			float mouseDist = (drp.transform.position - mouseHit.point).magnitude;

			if (charDist <= CharacterPickupDist && mouseDist <= MousePickupDist)
			{
				res.Add(drp);
				Debug.Log(drp);
			}
		}

		return res.ToArray();
	}

	public InputActionMap GetInputAction()
	{
		return _InputActionMap;
	}

	private void ControlMovementOnInput()
	{
		Vector2 dir = _MovementAction.ReadValue<Vector2>();

		MovementBasics.UpdateForwardsDir(dir.y);
		MovementBasics.UpdateTurning(dir.x);
	}

	public void Awake()
	{
		_InputActionMap = Controls.FindActionMap("gameplay");
	}

	public void Start()
	{
		_MovementAction = _InputActionMap.FindAction("movement");
		_CameraObject = GameObject.FindGameObjectWithTag("MainCamera");
		_Camera = _CameraObject.GetComponent<Camera>();

		DetectBackpacks()[0].AddItem(ItemManager.GetManager().GetItem("test-item"), 1);
		DetectBackpacks()[0].AddItem(ItemManager.GetManager().GetItem("test-item-2"), 1);
	}

	protected override void Update()
	{
		ControlMovementOnInput();
		GetSelectableDrops();
	}
}
