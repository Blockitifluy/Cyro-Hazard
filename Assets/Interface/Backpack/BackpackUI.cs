using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BackpackUI : MonoBehaviour
{
	private class BackpackPair
	{
		public Backpack Backpack;

		internal BackpackPair(Backpack backpack)
		{
			Backpack = backpack;
		}
	}

	private float _Timer = 0.0f;
	private float _LastUpdate = 0.0f;
	private List<Backpack> _LastBackpackCall = new();
	private List<BackpackPair> _Pairs = new();

	private GameObject _PlayerObject;
	private PlayerBehaviour _PlayerBehaviour;
	private InputActionMap _PlayerInput;
	private InputAction _OpenAction;
	private UIDocument _UIDoc;

	public float TimeBetweenUpdate = 0.9f;
	public int ItemUISize = 100;
	[Header("UI")]
	public VisualTreeAsset SubBackpack;
	public VisualTreeAsset ItemUI;

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		_UIDoc.enabled = !_UIDoc.enabled;
	}

	public void Awake()
	{
		_UIDoc = GetComponent<UIDocument>();
	}

	// Start is called before the first frame update
	public void Start()
	{
		_PlayerObject = GameObject.FindGameObjectWithTag("Player");
		_PlayerBehaviour = _PlayerObject.GetComponent<PlayerBehaviour>();
		_PlayerInput = _PlayerBehaviour.GetInputAction();
		_OpenAction = _PlayerInput.FindAction("open-backpack");
		_OpenAction.performed += ToggleMenu;
	}

	// TODO - ADD BACKPACK UI

	public void ClearBackpack()
	{
		var Selection = _UIDoc.rootVisualElement.Q<ListView>("Selection");
		Selection.hierarchy.Clear();
	}

	private void LoadItemUI(StoredItem stored, VisualElement container, Backpack backpack)
	{
		var itemInstance = ItemUI.Instantiate();

		Vector2Int itemSize = stored.Item.Size,
		itemPos = stored.Position,
		backSize = backpack.Size;

		Vector2 UISize = (Vector2)itemSize / backSize * 100,
		UIPos = (Vector2)itemPos / backSize * 100;
		print(UISize);

		var item = itemInstance.Q("item");
		item.style.width = new Length(UISize.x, LengthUnit.Percent);
		item.style.height = new Length(UISize.y, LengthUnit.Percent);

		item.style.left = new Length(UIPos.x, LengthUnit.Percent);
		item.style.top = new Length(UIPos.y, LengthUnit.Percent);

		var title = itemInstance.Q<Label>("title");
		title.text = stored.Item.Name;

		var amount = itemInstance.Q<Label>("amount");
		amount.text = $"({stored.Amount})";

		container.hierarchy.Add(itemInstance);
	}

	private void LoadBackpackUI(Backpack backpack, ListView selection)
	{
		var packUI = SubBackpack.Instantiate();

		var packLabel = packUI.Q<Label>("title");
		packLabel.text = backpack.Name;

		var weightLabel = packUI.Q<Label>("weight");
		weightLabel.text = $"{backpack.CurrentWeight}/{backpack.MaxWeight}";

		var itemContainer = packUI.Q("item-container");
		itemContainer.style.height = ItemUISize * backpack.Size.y;

		selection.hierarchy.Add(packUI);
		foreach (StoredItem strd in backpack.StoredItems)
			LoadItemUI(strd, itemContainer, backpack);
	}

	public void UpdateBackpack()
	{
		var root = _UIDoc.rootVisualElement;

		if (root == null)
			return;

		var selection = root.Q<ListView>("Selection");
		var backpacks = _PlayerBehaviour.DetectBackpacks();

		ClearBackpack();

		foreach (Backpack pack in backpacks)
			LoadBackpackUI(pack, selection);
	}

	// Update is called once per frame
	public void LateUpdate()
	{
		_Timer += Time.deltaTime;

		bool timeToRefresh = _Timer - _LastUpdate >= TimeBetweenUpdate;
		if (timeToRefresh && _UIDoc.enabled)
		{
			UpdateBackpack();
			_LastUpdate = _Timer;
		}
	}
}
