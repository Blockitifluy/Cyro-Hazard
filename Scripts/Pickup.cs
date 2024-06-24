using Godot;

[GlobalClass]
public partial class Pickup : Node3D
{
	/// <summary>
	/// The item code of the pickup
	/// </summary>
	[Export] public Items.ItemCode ItemCode { get; set; }
	/// <summary>
	/// The amount of items the pickup stores
	/// </summary>
	[Export] public int Amount { get; set; } = 1;

	public Items.ItemData Item {
		get { return Items.CodeToItem(ItemCode); }
		set {
			int index = Items.ItemMap.BinarySearch(value);
			ItemCode = (Items.ItemCode)index;
		}
	}
	
	private Label3D Label;
	private int LastPickupInput;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Label = GetNode<Label3D>("Label");
		GD.Print($"Item Made {Item}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Items.ItemData Item = Items.CodeToItem(ItemCode);
		Label.Text = $"{Item} ({Amount})";
	}
}
