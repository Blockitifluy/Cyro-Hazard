using Godot;
using System;
using System.Collections.Generic;

public partial class Backpack : Panel
{

  [ExportGroup("Preview")]
  [Export] public TextureRect PreviewImage;
  [Export] public Label ItemName;
  [Export] public Label Description;

  [ExportGroup("Items")]
  [Export] public string ItemFramePath;
  [Export] public FlowContainer ItemFlow;

  private Player PlayerController;
  private readonly List<TextureButton> ItemFrames = new();

  private void ClearAllFrames()
  {
    foreach (Control frame in ItemFrames)
    {
      frame.QueueFree();
    }

    ItemFrames.Clear();
  }

  private Vector2 Vector2ToContainer(Vector2 pos)
  {
    Inventory inventory = PlayerController.inventory;

    Vector2 scaled = inventory.Size / (pos / ItemFlow.Size);

    return scaled;
  }

  private void ChangePreview(Inventory.InventoryItem item)
  {
    Items.ItemData itemData = Items.CodeToItem(item.ItemCode);
    ItemName.Text = itemData.Name;
    Description.Text = itemData.Tooltip;

    // TODO Preview image
  }

  private void LoadItemFrame(Vector2I pos, Inventory.InventoryItem item)
  {
    PackedScene itemFrameScene = GD.Load<PackedScene>(ItemFramePath);

    Items.ItemData itemData = Items.CodeToItem(item.ItemCode);

    TextureButton itemFrame = itemFrameScene.Instantiate<TextureButton>();
    itemFrame.Position = Vector2ToContainer(pos); //TODO
    itemFrame.Size = Vector2ToContainer(itemData.Size);
    itemFrame.Pressed += () =>
    {
      GD.Print($"Previewing item {item}");
      ChangePreview(item);
    };

    // TODO Add images to TextureButton

    Label amountLabel = (Label)itemFrame.GetChild(0);
    amountLabel.Text = $"{item.Amount}";

    ItemFrames.Add(itemFrame);
    ItemFlow.AddChild(itemFrame);
  }

  private void UpdateUI(object sender, EventArgs e)
  {
    ClearAllFrames();

    Inventory inventory = PlayerController.inventory;
    foreach (KeyValuePair<Vector2I, Inventory.InventoryItem> pair in inventory.Placements)
    {
      var (pos, item) = (pair.Key, pair.Value);

      LoadItemFrame(pos, item);
    }

    GD.Print("Updated UI");
  }

  public override void _Ready()
  {
    base._Ready();

    Visible = false;

    PlayerController = (Player)GetTree().GetFirstNodeInGroup("Player");

    PlayerController.inventory.OnPlacementsUpdated += UpdateUI;
  }

  private void ToggleBackpack()
  {
    bool toggleBackpack = Input.IsActionJustPressed("toggle-backpack");
    if (toggleBackpack)
      Visible = !Visible;
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    ToggleBackpack();
  }
}
