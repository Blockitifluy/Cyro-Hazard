using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Inventory
{
  /// <summary>
  /// This exception is used when adding an item, the item doesn't fit
  /// </summary>
  [Serializable]
  public class DoesNotFitException : Exception
  {
    public DoesNotFitException() { }
    public DoesNotFitException(string message) : base(message) { }
    public DoesNotFitException(string message, Exception inner) : base(message, inner) { }
    protected DoesNotFitException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }

  public event EventHandler OnPlacementsUpdated;

  /// <summary>
  /// An item placed in the inventory
  /// </summary>
  public struct InventoryItem
  {
    public Vector2I Position;
    public readonly Items.ItemCode ItemCode;
    public int Amount;
    public bool Active = false;

    public override readonly string ToString()
    {
      Items.ItemData item = Items.CodeToItem(ItemCode);
      return item.ToString();
    }

    internal InventoryItem(Items.ItemCode code, Vector2I startingPos, int amount)
    {
      Position = startingPos;
      ItemCode = code;
      Amount = amount;
    }
  }

  /// <summary>
  /// The inventory's size
  /// </summary>
  public Vector2I Size = new(4, 4);

  public override bool Equals(object obj)
  {
    if (obj is Dictionary<Vector2I, InventoryItem>)
      return Placements == obj;
    return false;
  }

  public readonly Dictionary<Vector2I, InventoryItem> Placements = new();

  /// <summary>
  /// Gets the occupancy pf the inventory. False is empty; true is taken.
  /// </summary>
  /// <returns>An occupancy table</returns>
  public bool[,] GetOccupancy()
  {
    bool[,] Occupancy = new bool[Size.X, Size.Y];

    Parallel.ForEach(Placements, (pair) =>
    {
      var (pos, inventItem) = (pair.Key, pair.Value);

      Items.ItemData item = Items.CodeToItem(inventItem.ItemCode);

      int area = item.Size.X * item.Size.Y;

      for (int local = 0; local < area; local++)
      {
        int X = (local % item.Size.X) + pos.X,
        Y = (local / item.Size.X) + pos.Y;

        Occupancy[X, Y] = true;
      }
    });

    return Occupancy;
  }

  /// <summary>
  /// Checks if a slots (that can fit an item is taken up)
  /// </summary>
  /// <see cref="IsSlotTakenUp(Vector2I)"/>
  /// <param name="start">The search position</param>
  /// <param name="size">The search size</param>
  /// <returns>True if the space is free; false if taken.</returns>
  public bool AreSlotsTakenUp(Vector2I start, Vector2I size)
  {
    bool[,] Occupancy = GetOccupancy();

    int slotArea = size.X * size.Y;
    for (int i = 0; i < slotArea; i++)
    {
      int X = (i % size.X) + start.X,
      Y = Mathf.FloorToInt(i / size.X) + start.Y;

      bool taken = Occupancy[X, Y];

      if (taken) return true;
    }

    return false;
  }

  /// <summary>
  /// Get the search bounding box for an item
  /// </summary>
  /// <param name="size">The item's size</param>
  /// <returns>The bounding box</returns>
  public Vector2I GetInventoryBoundsForSize(Vector2I size)
  {
    int x = Size.X - size.X,
    y = Size.Y - size.Y;

    return new(x, y);
  }

  /// <summary>
  /// Adds an item to the inventory.
  /// </summary>
  /// <param name="code">The item code</param>
  /// <param name="at">The placement position</param>
  /// <param name="amount">The amount of items in a stack</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the amount is out of range of the max amount or at is out of bounds</exception>
  /// <exception cref="DoesNotFitException">The wanted slot is taken up</exception>
  public void AddItem(Items.ItemCode code, Vector2I at, int amount)
  {
    Items.ItemData itemData = Items.CodeToItem(code);
    Vector2I size = itemData.Size;

    if (amount > itemData.MaxAmount)
      throw new ArgumentOutOfRangeException(nameof(amount));

    if (at.X >= Size.X || at.Y >= Size.Y)
      throw new ArgumentOutOfRangeException(nameof(at));

    if (AreSlotsTakenUp(at, size))
      throw new DoesNotFitException(nameof(at));

    InventoryItem inventoryItem = new(code, at, amount);
    Placements.Add(at, inventoryItem);

    OnPlacementsUpdated?.Invoke(this, EventArgs.Empty);
  }

  /// <summary>
  /// Checks if a single slot is taken up.
  /// </summary>
  /// <see cref="AreSlotsTakenUp(Vector2I, Vector2I)"/>
  /// <param name="slot">The slot position to be searched</param>
  /// <returns>If a slot is taken up by another item</returns>
  public bool IsSlotTakenUp(Vector2I slot)
  {
    bool[,] Occupancy = GetOccupancy();

    return Occupancy[slot.X, slot.Y];
  }

  /// <summary>
  ///  Tries to find a space for an item to be placed.
  /// </summary>
  /// <param name="size">The size of the item</param>
  /// <returns>A space where an item can be placed. Returns <c>Vector2I.MinValue</c> if no space was found.</returns>
  public Vector2I FindSpaceFor(Vector2I size)
  {
    Vector2I bounds = GetInventoryBoundsForSize(size);

    for (int i = 0; i < bounds.X * bounds.Y; i++)
    {
      int tlx = i % bounds.X,
      tly = i / bounds.X;

      Vector2I pos = new(tlx, tly);

      bool isTaken = AreSlotsTakenUp(pos, size);
      if (!isTaken) return pos;
    }

    // Nothing found
    return Vector2I.MinValue;
  }

  public Inventory(Vector2I size)
  {
    Size = size;
  }

  public override int GetHashCode()
  {
    return Placements.GetHashCode();
  }
}
