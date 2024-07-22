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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Label = GetNode<Label3D>("Label");
		meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		mesh = GetItemsMesh(ItemCode);

		meshInstance.Mesh = mesh;

		GD.PrintRich($"[b][color=PURPLE]Item[/color][/b] Made {Item.Name}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Items.ItemData Item = Items.CodeToItem(ItemCode);
		Label.Text = $"{Item} ({Amount})";
	}
}
