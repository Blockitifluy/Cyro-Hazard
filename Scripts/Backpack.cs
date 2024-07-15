using Godot;
using System;
using System.Collections.Generic;

public partial class Backpack : Panel
{
  [ExportGroup("Preview")]
  [Export] public TextureRect PreviewImage;
  [Export] public Label ItemName;
  [Export] public Label Description;
  [Export] public Button EquipButton;

  [ExportGroup("Items")]
  [Export] public PackedScene ItemFrame;
  [Export] public Control ItemContainer;

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

  private Vector2 Vector2ToContainer(Vector2 vector)
  {
    Inventory inventory = PlayerController.inventory;

    Vector2 ratio = ItemContainer.Size / (Vector2)inventory.Size;

    Vector2 scaled = vector * ratio;

    return scaled;
  }

  public Action LastEquipSub;

  private void ChangePreview(Inventory.InventoryItem item)
  {
    Items.ItemData itemData = Items.CodeToItem(item.ItemCode);

    ItemName.Text = itemData.Name;
    Description.Text = itemData.Tooltip;

    EquipButton.Visible = itemData.Equipable;
    if (LastEquipSub != null) EquipButton.Pressed -= LastEquipSub;

    void EquipSubscription()
    {
      PlayerController.Hotbar.AddToFromHotbar(item, 1);
    }

    LastEquipSub = EquipSubscription;
    EquipButton.Pressed += EquipSubscription;
  }

  private void LoadItemFrame(Vector2I pos, Inventory.InventoryItem item)
  {
    Items.ItemData itemData = Items.CodeToItem(item.ItemCode);

    TextureButton itemFrame = ItemFrame.Instantiate<TextureButton>();
    itemFrame.Position = Vector2ToContainer(pos); //TODO
    itemFrame.Size = Vector2ToContainer(itemData.Size);
    itemFrame.Pressed += () =>
    {
      GD.PrintRich($"[b][color=PURPLE]Item[/color][/b] Previewing {item}");
      ChangePreview(item);
    };

    // TODO Add images to TextureButton

    Label amountLabel = itemFrame.GetChild<Label>(0);
    amountLabel.Text = item.Amount.ToString();

    ItemFrames.Add(itemFrame);
    ItemContainer.AddChild(itemFrame);
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

    GD.PrintRich("[b][color=PURPLE]Item[/color][/b] Updated UI");
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
