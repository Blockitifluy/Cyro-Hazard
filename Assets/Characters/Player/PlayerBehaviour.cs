using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using CH.Items;

namespace CH.Character.Player
{
	public class PlayerBehaviour : CharacterControl
	{
		private InputActionMap _InputActionMap;
		private InputAction _MovementAction;
		private GameObject _CameraObject;
		private GameObject _Pivot;
		private Camera _Camera;

		[Header("Controls")]
		public InputActionAsset Controls;
		public Vector2 Senitivity;

		[Header("Pickup")]
		public float CharacterPickupDist = 5;
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
			MovementBasics.MovementDirection = dir;
		}

		private void MoveCamera()
		{
			Vector2 mouseDir = Senitivity * Time.deltaTime * Mouse.current.delta.ReadValue().normalized;

			transform.Rotate(Vector3.up * mouseDir.x);

			_Pivot.transform.Rotate(Vector3.right * mouseDir.y);
		}

		public override void Awake()
		{
			base.Awake();
			_InputActionMap = Controls.FindActionMap("gameplay");
		}

		public void Start()
		{
			_MovementAction = _InputActionMap.FindAction("movement");
			_CameraObject = GameObject.FindGameObjectWithTag("MainCamera");
			_Camera = _CameraObject.GetComponent<Camera>();
			_Pivot = GameObject.FindGameObjectWithTag("CameraHandle");

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = true;

			DetectBackpacks()[0].AddItem(ItemManager.GetManager().GetItem("test-item"), 1);
			DetectBackpacks()[0].AddItem(ItemManager.GetManager().GetItem("test-item"), 1);
		}

		public void Update()
		{
			ControlMovementOnInput();
			GetSelectableDrops();
			MoveCamera();
		}
	}
}