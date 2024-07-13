using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Pickup : RigidBody3D
{
	private Items.ItemCode itemCode;
	/// <summary>
	/// The item code of the pickup
	/// </summary>
	[Export]
	public Items.ItemCode ItemCode
	{
		get { return itemCode; }
		set
		{
			Item = Items.CodeToItem(value);
			itemCode = value;
		}
	}

	/// <summary>
	/// The amount of items the pickup stores
	/// </summary>
	[Export] public int Amount { get; set; } = 1;

	public Items.ItemData Item;

	private Label3D Label;
	private int LastPickupInput;
	private MeshInstance3D meshInstance;
	private Mesh mesh;

	[Export]
	static public readonly Dictionary<Items.ItemCode, string> CodeToPath = new() {
		{Items.ItemCode.Wood, "res://Meshs/WoodItem.obj"},
		{Items.ItemCode.Stone, "res://Meshs/StoneItem.obj"}
	};

	static public Mesh CodeToMesh(Items.ItemCode code)
	{
		string path = CodeToPath[code];

		Mesh mesh = GD.Load<Mesh>(path);

		return mesh;
	}

	static private Mesh GetMeshFromItemCode(Items.ItemCode code)
	{
		var itemMesh = CodeToMesh(code);

		return itemMesh;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Label = GetNode<Label3D>("Label");
		meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		mesh = CodeToMesh(ItemCode);

		meshInstance.Mesh = mesh;

		GD.Print($"Item Made {Item}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Items.ItemData Item = Items.CodeToItem(ItemCode);
		Label.Text = $"{Item} ({Amount})";
	}
}
