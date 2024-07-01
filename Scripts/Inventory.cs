using Godot;
using System;
using System.Collections.Generic;

public class Inventory
{
  public struct InventoryItem
  {
    public Vector2I Position;
    public Items.ItemCode ItemCode;
    public int Amount;
    public bool Active = false;

    internal InventoryItem(Items.ItemCode code, Vector2I startingPos, int amount)
    {
      Position = startingPos;
      ItemCode = code;
      Amount = amount;
    }
  }

  public Vector2I Size = new(4, 4);

  public Dictionary<Vector2I, InventoryItem> Placements = new();

  public bool[,] GetOccupancy()
  {
    bool[,] Occupancy = new bool[Size.X, Size.Y];

    foreach (var (pos, inventItem) in Placements)
    {
      Items.ItemData item = Items.CodeToItem(inventItem.ItemCode);

      int area = item.Size.X * item.Size.Y;

      for (int local = 0; local < area; local++)
      {
        int X = (local % item.Size.X) + pos.X,
        Y = (int)Mathf.Floor(local / item.Size.X) + pos.Y;

        Occupancy[X, Y] = true;
      }
    }

    return Occupancy;
  }

  public bool AreSlotsTakenUp(Vector2I start, Vector2I size)
  {
    bool[,] Occupancy = GetOccupancy();

    int slotArea = size.X * size.Y;
    for (int local = 0; local < slotArea; local++)
    {
      int X = (local % size.X) + start.X,
      Y = (int)Mathf.Floor(local / size.X) + start.Y;

      bool taken = Occupancy[X, Y];

      if (taken) return true;
    }

    return false;
  }

  public void AddItem(Items.ItemCode code, Vector2I at, int amount)
  {
    Items.ItemData itemData = Items.CodeToItem(code);
    Vector2I size = itemData.Size;

    if (at.X >= Size.X || at.Y >= Size.Y)
      throw new Exception("at is out of bounds");

    if (AreSlotsTakenUp(at, size))
      throw new Exception($"Item {itemData} doesn't fit");

    InventoryItem inventoryItem = new(code, at, amount);
    Placements.Add(at, inventoryItem);
  }

  public bool IsSlotTakenUp(Vector2I slot)
  {
    // First Array is y, Second is x
    bool[,] Occupancy = GetOccupancy();

    return Occupancy[slot.X, slot.Y];
  }

  public Inventory(Vector2I size)
  {
    Size = size;
  }
}
