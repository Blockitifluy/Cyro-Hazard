using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public abstract partial class BaseTool : Node3D
{
  private static Dictionary<Items.ItemCode, PackedScene> _ToolCache = new();

  public static T GetToolFromCode<T>(Items.ItemCode code) where T : BaseTool
  {
    if (_ToolCache.ContainsKey(code))
      return _ToolCache[code].Instantiate<T>();

    Items.ItemData itemBase = Items.CodeToItem(code);

    string scenePath = itemBase.ToolScene;
    PackedScene scene = GD.Load<PackedScene>(scenePath);
    _ToolCache.Add(code, scene);

    return scene.Instantiate<T>();
  }

  public static BaseTool GetToolFromCode(Items.ItemCode code)
  {
    return GetToolFromCode<BaseTool>(code);
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

  protected T GetUser<T>() where T : BasicCharacter
  {
    T user = this.GetParentByType<T>();

    if (user == null)
      throw new NullReferenceException($"User of {Name} was null.");

    return user;
  }

  protected BasicCharacter GetUser()
  {
    return GetUser<BasicCharacter>();
  }

  /// <summary>
  /// Unequips the tool, use after using the node since it will automaticly delete itself.
  /// </summary>
  public virtual void Unequip()
  {
    inventoryItem.Active = false;
  }

  public override void _EnterTree()
  {
    base._EnterTree();
    Equip();
  }

  public override void _ExitTree()
  {
    base._ExitTree();
    Unequip();
  }

  public override void _Ready()
  {
    base._Ready();
  }
}