using UnityEngine.InputSystem;
using UnityEngine;
using CyroHazard.Items;
using CyroHazard.Items.Container;

namespace CyroHazard.Character.Player
{
	public class PlayerBehaviour : CharacterControl
	{
		private InputActionMap _InputActionMap;
		private InputAction _MovementAction;
		private InputAction _PickupAction;
		private GameObject _CameraObject;
		private GameObject _Pivot;
		private Camera _Camera;

		[Header("Controls")]
		public bool ControlCamera = true;
		public InputActionAsset Controls;
		public Vector2 Senitivity;

		[Header("Pickup")]
		public float CharacterPickupDist = 5;
		public float MousePickupDist = 2.5f;

		public InputActionMap GetInputAction()
		{
			return _InputActionMap;
		}

		public void TryToPickup(InputAction.CallbackContext _)
		{
			RaycastHit? nullableMouseHit = Helper.GetMouseRayHitInfo(_Camera);
			if (!nullableMouseHit.HasValue)
				return;
			var mouseHit = nullableMouseHit.Value;

			bool hasTag = GameObject.FindGameObjectWithTag("Dropped Items");
			if (!hasTag) return;
			Transform dropTrans = mouseHit.transform;

			float charDist = (transform.position - dropTrans.position).sqrMagnitude,
			mouseDist = (mouseHit.point - dropTrans.position).sqrMagnitude;

			bool charCheck = charDist <= CharacterPickupDist * CharacterPickupDist,
			mouseCheck = mouseDist <= MousePickupDist * MousePickupDist;

			if (charCheck && mouseCheck)
			{
				// TODO - Add surport for multiple Backpacks
				DroppedItem dropped = dropTrans.GetComponent<DroppedItem>();
				GridBackpack firstBackpack = GetFirstBackpack();
				dropped.PickupDropped(firstBackpack);
			}
		}

		private void ControlMovementOnInput()
		{
			Vector2 dir = _MovementAction.ReadValue<Vector2>();
			MovementBasics.MovementDirection = dir;
		}

		private void MoveCamera()
		{
			if (!ControlCamera) return;

			Vector2 mouseDir = Senitivity * Time.deltaTime * Mouse.current.delta.ReadValue().normalized;

			transform.Rotate(Vector3.up * mouseDir.x);

			_Pivot.transform.Rotate(Vector3.right * mouseDir.y);
		}

		public override void Awake()
		{
			base.Awake();
			_InputActionMap = Controls.FindActionMap("Player");
		}

		public void Start()
		{
			_MovementAction = _InputActionMap.FindAction("movement");
			_CameraObject = GameObject.FindGameObjectWithTag("MainCamera");
			_Camera = _CameraObject.GetComponent<Camera>();
			_Pivot = GameObject.FindGameObjectWithTag("CameraHandle");

			_PickupAction = _InputActionMap.FindAction("Pickup");

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = true;
		}

		public void Update()
		{
			ControlMovementOnInput();
			MoveCamera();

			_PickupAction.performed += TryToPickup;
		}
	}
}