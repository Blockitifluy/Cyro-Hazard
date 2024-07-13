using Godot;

[GlobalClass]
public abstract partial class Tool : StaticBody3D
{
  public static Tool GetToolFromCode(Items.ItemCode code)
  {
    switch (code)
    {
      case Items.ItemCode.Axe:
        return new Axe();
      default:
        throw new Items.InvalidItemCodeException("code couldn't be matched");
    }
  }

  /// <summary>
  /// Linked to the public accessor of <see cref="InventoryItem"/>
  /// </summary>
  private Inventory.InventoryItem inventoryItem;

  /// <summary>
  /// The linked item / tool in the inventory.
  /// </summary>
  public Inventory.InventoryItem InventoryItem
  {
    get { return inventoryItem; }
    set
    {
      if (!Items.IsCodeValid(value.ItemCode))
        throw new Items.InvalidItemCodeException("Tried to set invalid item code");

      inventoryItem = value;
    }
  }

  /// <summary>
  /// Equips the item, automatic done when node is ready.
  /// </summary>
  protected virtual void Equip()
  {
    inventoryItem.Active = true;
  }

  /// <summary>
  /// Unequips the tool, use after using the node since it will automaticly delete itself.
  /// </summary>
  public virtual void Unequip()
  {
    inventoryItem.Active = false;
    QueueFree();
  }

  public override void _EnterTree()
  {
    base._EnterTree();
    Equip();
  }

  public override void _Ready()
  {
    base._Ready();

    // TODO Mesh
    MeshInstance3D meshInstance = new();
    CallDeferred("add_child", meshInstance);
  }
}