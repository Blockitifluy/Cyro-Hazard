using System;
using System.Collections.Generic;
using System.Text;
using Godot;

public class Items
{
  [Serializable]
  public class InvalidItemCodeException : Exception
  {
    public InvalidItemCodeException() { }
    public InvalidItemCodeException(string message) : base(message) { }
    public InvalidItemCodeException(string message, Exception inner) : base(message, inner) { }
    protected InvalidItemCodeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }

  [Serializable]
  public class ItemNotEquipableException : Exception
  {
    public ItemNotEquipableException() { }
    public ItemNotEquipableException(string message) : base(message) { }
    public ItemNotEquipableException(string message, Exception inner) : base(message, inner) { }
    protected ItemNotEquipableException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }

  public readonly struct ItemData
  {
    /// <summary>
    /// The max stack of an Item
    /// </summary>
    readonly public int MaxAmount;
    /// <summary>
    /// The name of the Item
    /// </summary>
    readonly public string Name = "Unnamed Item";
    /// <summary>
    /// Is it equipable
    /// </summary>
    readonly public bool Equipable = false;
    readonly public string Tooltip = "";
    readonly public string ToolScene = "";

    readonly public Vector2I Size;

    public override string ToString()
    {
      if (string.IsNullOrWhiteSpace(Name))
        return "";

      StringBuilder newText = new(Name.Length * 2);
      newText.Append(Name[0]);
      for (int i = 1; i < Name.Length; i++)
      {
        if (char.IsUpper(Name[i]) && Name[i - 1] != ' ')
          newText.Append(' ');
        newText.Append(Name[i]);
      }
      return newText.ToString();
    }

    /// <summary>
    /// Creates a new item data 
    /// </summary>
    /// <param name="name">The item's name</param>
    /// <param name="isEquipable">Is the item equipable</param>
    /// <param name="size">The size of the item in the backpack</param>
    /// <param name="tooltip">The description of the item</param>
    /// <param name="maxAmount">The max amount that can be stored in a stack, the default is 99</param>
    /// <exception cref="ArgumentOutOfRangeException">The maxAmount is less than 0</exception>
    internal ItemData(string name, bool isEquipable, Vector2I size, string tooltip = "[No Tooltip]", int maxAmount = 99, string toolScene = "")
    {
      if (maxAmount <= 0)
        throw new ArgumentOutOfRangeException(nameof(maxAmount));

      MaxAmount = maxAmount;
      Name = name;
      Size = size;
      Tooltip = tooltip;
      Equipable = isEquipable;
      ToolScene = toolScene;
    }
  }

  public static ItemData CodeToItem(ItemCode code)
  {
    int ItemToCode = (int)code;
    if (!ItemMap.ContainsKey(code))
      throw new InvalidItemCodeException($"{ItemToCode} code doesn't exist for item");

    return ItemMap[code];
  }

  public static bool IsCodeValid(ItemCode code)
  {
    return ItemMap.ContainsKey(code);
  }

  /// <summary>
  /// The enum code of an item
  /// </summary>
  public enum ItemCode : int
  {
    Wood = 0,
    Stone = 1,
    Axe = 2,
    Snow = 3,
    Shovel = 4,
    ThisWillThrowErrorIfUsed = 99999999
  }

  /// <summary>
  /// A list of every Item
  /// </summary>
  static readonly public Dictionary<ItemCode, ItemData> ItemMap = new()
  {
    {ItemCode.Wood, new("Wood", false, new(2, 1), "A flexible and strong material from trees")},
    {ItemCode.Stone, new("Stone", false, new(1,1), "A strong rock mined from underground and grinded by snow")},
    {ItemCode.Axe, new("Axe", true, new(2, 3), "Chops Trees", 1, "res://Items/Tools/Axe/Axe.tscn")},
    {ItemCode.Snow, new("Snow", false, new(1,1), "A collection of ice crystals laying on ground that can grinded")},
    {ItemCode.Shovel, new("Shovel", true, new(1,3), "Can shovel up snow of the ground", 1, "res://Items/Tools/Shovel/Shovel.tscn")}
  };
}