using System;
using Godot;

[GlobalClass]
public partial class Pickup : RigidBody3D
{
	private Items.ItemCode _ItemCode;
	/// <summary>
	/// The item code of the pickup
	/// </summary>
	[Export]
	public Items.ItemCode ItemCode
	{
		get { return _ItemCode; }
		set
		{
			Item = Items.CodeToItem(value);
			_ItemCode = value;
		}
	}

	/// <summary>
	/// The amount of items the pickup stores
	/// </summary>
	[Export] public int Amount { get; set; } = 1;

	public Items.ItemData Item;

	private Label3D _Label;
	private int LastPickupInput;
	private MeshInstance3D _MeshInstance;
	private Mesh _Mesh;

	/// <summary>
	/// Gets an item's mesh.
	/// </summary>
	/// <remarks>The item must be in <c>res://Items/Pickup/Meshs/</c> directory
	/// and be named <c>{Item's Name}Item.obj</c>.</remarks>
	/// <param name="code">The item's code</param>
	/// <returns>The item's mesh</returns>
	static public Mesh GetItemsMesh(Items.ItemCode code)
	{
		const string basePath = "res://Items/Pickup/Meshs/";

		Items.ItemData baseItem = Items.CodeToItem(code);

		string expectedName = $"{baseItem.Name}Item.obj";

		return GD.Load<Mesh>(basePath + expectedName);
	}

	static public readonly string PickupGroup = "Pickups";

	static public Pickup DropItem(Vector3 pos, Items.ItemCode code, int amount, SceneTree tree)
	{
		Items.ItemData baseItem = Items.CodeToItem(code);

		if (amount > baseItem.MaxAmount || amount <= 0)
			throw new ArgumentOutOfRangeException(nameof(amount));

		const string pickupScenePath = "res://Items/Pickup/Pickup.tscn";

		PackedScene scene = GD.Load<PackedScene>(pickupScenePath);

		Pickup pickup = scene.Instantiate<Pickup>();
		pickup.ItemCode = code;
		pickup.Amount = amount;
		pickup.Position = pos;
		tree.Root.AddChild(pickup);

		pickup.AddToGroup(PickupGroup);

		return pickup;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		_Label = GetNode<Label3D>("Label");
		_MeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		_Mesh = GetItemsMesh(ItemCode);

		Item = Items.CodeToItem(ItemCode);

		if (_Mesh != null)
			_MeshInstance.Mesh = _Mesh;

		_Label.Text = $"{Item} ({Amount})";

		GD.Print(_Mesh);

		GD.PrintRich($"[b][color=PURPLE]Item[/color][/b] Made {Item.Name}");
	}
}
