using System;
using System.Collections.Generic;
using UnityEngine;

using OccupancyID = System.UInt64;

namespace CyroHazard.Items.Container
{
	/// <summary>
	/// This class refers to anything that can contain items
	/// and has a grid system that is not the ground, including:
	/// <list type="bullet">
	/// <item>Player Inventories,</item>
	/// <item>Chest / Containers</item>
	/// </list>
	/// </summary>
	[AddComponentMenu("Items/Container/Grid Backpack")]
	public class GridBackpack : MonoBehaviour
	{
		/// <summary>
		/// A list that contains all items in the <see cref="GridBackpack"/>,
		/// this doesn't account for size and position.
		/// </summary>
		private readonly List<StoredItem> _StoredItems = new();

		/// <summary>
		/// The maxiumun amount of slots a backpack can have, this is because ulong only has 64 bits for occupancy.
		/// </summary>
		const int MaxSlotCount = 64;

		/// <summary>
		/// The name of the backpack
		/// </summary>
		public string Name = "Unnamed Backpack";

		/// <summary>
		/// The size of the backpack. If the area of the size variable is over 64,
		/// it's will be incorrect
		/// </summary>
		/// <remarks>The reason why it can't go over 64 in area, is because it uses the ulong type</remarks>
		[Header("Carrying")]
		public Vector2Int Size = new(8, 8);
		/// <summary>
		/// The maxiunum weight that the it can carry
		/// </summary>
		public float MaxWeight = 1000.0f;
		/// <summary>
		/// If true, the backpack can exceed it's <see cref="MaxWeight"/>
		/// </summary>
		public bool CanOverweight = true;

		public List<StoredItem> StoredItems
		{
			get { return new(_StoredItems); }
		}

		/// <summary>
		/// The area the backpack
		/// </summary>
		/// <example>7x8 = 56</example>
		public int Area => Size.x * Size.y;

		/// <summary>
		/// The current weight of every item in the backpack.
		/// </summary>
		public float CurrentWeight
		{
			get
			{
				float weight = 0.0f;
				foreach (StoredItem strd in _StoredItems)
					weight += strd.Item.Weight;
				return weight;
			}
		}

		/// <summary>
		/// Fired if the backpack updated for any reason.
		/// </summary>
		public event EventHandler BackpackUpdated;

		// Converting Index to Backpack slot

		/// <summary>
		/// Converts a single int index to a location with a x and y.
		/// </summary>
		/// <param name="i">The index of the slot</param>
		/// <returns>A tuple of two integers</returns>
		/// <exception cref="OverflowException">If the backpack's size exceeds an area of 64</exception>
		public (int, int) IndexToXY(int i)
		{
			if (Area > MaxSlotCount)
				throw new OverflowException($"Backpack size can't be over {MaxSlotCount} bits");

			int x = i % Size.x,
			y = i / Size.x;

			return (x, y);
		}

		/// <inheritdoc cref="IndexToXY"/>
		/// <returns>A Vector2Int</returns>
		public Vector2Int IndexToVector(int i)
		{
			var (x, y) = IndexToXY(i);

			return new(x, y);
		}

		/// <summary>
		/// Convert x and y to a slot's index.
		/// </summary>
		/// <param name="x">The x axis..</param>
		/// <param name="y">The y axis..</param>
		/// <returns>The slot's index.</returns>
		public int XYToIndex(int x, int y)
		{
			return (y * Size.x) + x;
		}

		/// <inheritdoc cref="XYToIndex"/>
		/// <param name="pos">The position of the slot</param>
		public int XYToIndex(Vector2Int pos)
		{
			return XYToIndex(pos.x, pos.y);
		}

		// Occupancy

		/// <summary>
		/// Get the occupancy of every tile in the backpack.
		/// </summary>
		/// <remarks>Occupancy refers to how a slot is taken up by an item</remarks>
		/// <returns></returns>
		public OccupancyID GetOccupancy()
		{
			OccupancyID occupancy = 0;

			foreach (StoredItem strd in _StoredItems)
			{
				BaseItem item = strd.Item;
				Vector2Int pos = strd.Position;
				int area = item.Size.x * item.Size.y;

				int topCorner = XYToIndex(pos);
				for (int x = pos.x; x < item.Size.x + pos.x; x++)
				{
					for (int y = pos.y; y < item.Size.y + pos.y; y++)
					{
						occupancy |= 1ul << XYToIndex(x, y);
					}
				}
			}

			return occupancy;
		}

		/// <summary>
		/// This function is similar to <see cref="GetOccupancy"/>, but the slots
		/// are <see cref="StoredItem"/>s instead of single bits.
		/// </summary>
		/// <returns>A two dimensional list of <see cref="StoredItem"/>s.</returns>
		public StoredItem[,] GetOccupancyWithItem()
		{
			StoredItem[,] occupancy = new StoredItem[Size.x, Size.y];

			foreach (StoredItem strd in _StoredItems)
			{
				BaseItem item = strd.Item;
				Vector2Int pos = strd.Position;
				int area = item.Size.x * item.Size.y;

				int itemPosIndex = pos.x + pos.y * Size.x;
				for (int i = itemPosIndex; i < area + itemPosIndex; i++)
				{
					var (x, y) = IndexToXY(i);
					occupancy[x, y] = strd;
				}
			}

			return occupancy;
		}

		/// <summary>
		/// Checks it a space in a rectangle is entirely filled with free spaces.
		/// </summary>
		/// <param name="at">The top-left location.</param>
		/// <param name="size">The size of the rectangle.</param>
		/// <returns>True if all slots are occupied.</returns>
		public bool AreSlotsOccupied(Vector2Int at, Vector2Int size)
		{
			OccupancyID occupancy = GetOccupancy();

			return AreSlotsOccupied(at, size, occupancy);
		}

		/// <inheritdoc cref="AreSlotsOccupied(Vector2Int, Vector2Int)"/>
		/// <param name="occupancy">The backpack occupancy</param>
		public bool AreSlotsOccupied(Vector2Int at, Vector2Int size, OccupancyID occupancy)
		{
			int area = size.x * size.y;

			for (int i = 0; i < area; i++)
			{
				int x = i % size.x,
				y = i / size.x;

				int x2 = at.x + x,
				y2 = at.y + y;

				if (IsCoordOutOfRange(x2, y2)) return true;

				bool occupied = IsSlotOccupied(x2, y2, occupancy);

				if (occupied) return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if a slot is freed.
		/// </summary>
		/// <param name="x">The x axis.</param>
		/// <param name="y">The y axis.</param>
		/// <returns>True if the slot is occupied.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If a location is out of range.</exception>
		public bool IsSlotOccupied(int x, int y)
		{
			OccupancyID occupancy = GetOccupancy();

			return IsSlotOccupied(x, y, occupancy);
		}

		/// <inheritdoc cref="IsSlotOccupied(int, int)"/>
		/// <param name="at">The slot to be checked to be occupied.</param>
		public bool IsSlotOccupied(Vector2Int at)
		{
			OccupancyID occupancy = GetOccupancy();

			return IsSlotOccupied(at, occupancy);
		}

		/// <inheritdoc cref="IsSlotOccupied(int, int)"/>
		/// <param name="occupancy">The backpack occupancy</param>
		public bool IsSlotOccupied(int x, int y, OccupancyID occupancy)
		{
			if (IsCoordOutOfRange(x, y)) return true;

			int i = XYToIndex(x, y);

			return Helper.IsBitSet(occupancy, i);
		}

		/// <inheritdoc cref="IsSlotOccupied(Vector2Int)"/>
		/// <param name="occupancy">The backpack occupancy</param>
		public bool IsSlotOccupied(Vector2Int at, OccupancyID occupancy)
		{
			return IsSlotOccupied(at.x, at.y, occupancy);
		}

		/// <summary>
		/// Checks if a coorninate is in range
		/// </summary>
		/// <param name="x">The x axis.</param>
		/// <param name="y">The y axis.</param>
		/// <returns>True, if a coorninate is in range of the backpack size</returns>
		public bool IsCoordOutOfRange(int x, int y)
		{
			bool bx = 0 <= x && x < Size.x,
			by = 0 <= y && y < Size.y;
			return !(bx && by);
		}

		/// <inheritdoc cref="IsCoordOutOfRange(int, int)"/>
		/// <param name="coord">The slot's location.</param>
		public bool IsCoordOutOfRange(Vector2Int coord)
		{
			return IsCoordOutOfRange(coord.x, coord.y);
		}

		// Inserting

		/// <summary>
		/// Invoked when an item was added to the backpack.
		/// </summary>
		public event EventHandler<StoredItem> ItemAdded;

		/// <summary>
		/// Thrown if there is an error in the placement
		/// </summary>
		[Serializable]
		public class PlacementException : Exception
		{
			public PlacementException() { }
			public PlacementException(string message) : base(message) { }
			public PlacementException(string message, Exception inner) : base(message, inner) { }
			protected PlacementException(
				System.Runtime.Serialization.SerializationInfo info,
				System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		/// <summary>
		/// An enum used to provide a reason why a placement failed.
		/// `None` is onlt expection.
		/// </summary>
		public enum EPlacementReason : ushort
		{
			None = 0,
			Weight = 1,
			Occupied = 2,
		}

		/// <summary>
		/// Checks if an item can be placed inside in the backpack in a speific position.
		/// </summary>
		/// <remarks>Checks occupied slots and weight</remarks>
		/// <param name="pos">The position</param>
		/// <param name="item">The item</param>
		/// <param name="reason">Provides a reason the placement would have failed</param>
		/// <returns>True if the item can be placed there.</returns>
		public bool CanPlaceItem(Vector2Int pos, BaseItem item, out EPlacementReason reason)
		{
			if (AreSlotsOccupied(pos, item.Size))
			{
				reason = EPlacementReason.Occupied;
				return false;
			}

			float futureWeight = CurrentWeight + item.Weight;
			if (futureWeight > MaxWeight && !CanOverweight)
			{
				reason = EPlacementReason.Weight;
				return false;
			}

			reason = EPlacementReason.None;
			return true;
		}

		/// <inheritdoc cref="AddItemAt(StoredItem, Vector2Int)"/>
		/// <param name="item">The item to be added.</param>
		/// <param name="amount">The amount of stack.</param>
		/// <returns>The stored item.</returns>
		public StoredItem AddItemAt(BaseItem item, int amount, Vector2Int at)
		{
			StoredItem storedItem = new(item, amount, at);

			AddItemAt(storedItem, at);

			return storedItem;
		}

		/// <summary>
		/// Adds an item to the backpack.
		/// </summary>
		/// <param name="stored">The item to be added.</param>
		/// <param name="at">The location where item is going to added.</param>
		/// <exception cref="PlacementException"></exception>
		public void AddItemAt(StoredItem stored, Vector2Int at)
		{
			BaseItem item = stored.Item;
			if (!CanPlaceItem(at, item, out EPlacementReason reason))
				throw new PlacementException($"Can't place item (with size {item.Size}) at {at} (reason: {reason})");

			stored.Position = at;
			_StoredItems.Add(stored);

			ItemAdded?.Invoke(this, stored);
			BackpackUpdated?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Sees if there is space in the <see cref="GridBackpack"/> for the <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item wanting to be placed.</param>
		/// <param name="pos">The returning position.</param>
		/// <returns>If a space has been found for the <paramref name="item"/>.</returns>
		public bool CanFindPlacementFor(BaseItem item, out Vector2Int pos)
		{
			OccupancyID occupancy = GetOccupancy();

			int i = 0;
			do
			{
				pos = IndexToVector(i);
				bool occupied = AreSlotsOccupied(pos, item.Size, occupancy);
				if (!occupied)
					return true;
				i++;
			} while (i < Area);

			return false;
		}

		/// <inheritdoc cref="AddItem(StoredItem, bool)"/>		
		/// <param name="item">The item wanting to be placed.</param>
		/// <param name="amount">The amount of item.</param>
		/// <returns>The <seealso cref="StoredItem"/> that had been place.</returns>
		public StoredItem AddItem(BaseItem item, int amount, bool dropIfNotFound = false)
		{
			StoredItem stored = new(item, amount, -Vector2Int.one);

			AddItem(stored, dropIfNotFound);

			return stored;
		}

		/// <summary>
		/// Adds an item to the backpack, automatically finding a placement.
		/// </summary>
		/// <param name="stored">The stored item wanting to be placed.</param>
		/// <param name="dropIfNotFound">Drop the item, when no space is found.</param>
		/// <exception cref="PlacementException">Returned if no space had been found.</exception>
		public void AddItem(StoredItem stored, bool dropIfNotFound = false)
		{
			bool canFindPlace = CanFindPlacementFor(stored.Item, out Vector2Int at);

			if (canFindPlace)
			{
				AddItemAt(stored, at);
				return;
			}

			if (!dropIfNotFound)
				throw new PlacementException($"Couldn't find placement for {stored}");

			ItemManager itemManager = ItemManager.Manager;
			itemManager.CreateDroppedItem(stored.Item, stored.Amount, transform.position);

			return;
		}

		// Modifing

		public delegate void DItemModified(object sender, StoredItem oldItem, StoredItem newItem);
		/// <summary>
		/// Invoked when an item is modified. 
		/// </summary>
		public event DItemModified ItemModified;

		/// <summary>
		/// Thrown when an error when modifing an <see cref="StoredItem"/>.
		/// </summary>
		[Serializable]
		public class ModifingException : Exception
		{
			public ModifingException() { }
			public ModifingException(string message) : base(message) { }
			public ModifingException(string message, Exception inner) : base(message, inner) { }
			protected ModifingException(
				System.Runtime.Serialization.SerializationInfo info,
				System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		/// <summary>
		/// Changes an item's amount.
		/// </summary>
		/// <remarks>
		/// Also fires the <see cref="ItemModified"/> event.
		/// </remarks>
		/// <param name="stored">The StoredItem</param>
		/// <param name="newAmount">The new set amount</param>
		/// <exception cref="ModifingException">When the <paramref name="stored"/> wasn't found in the backpack or exceeds maxStack.</exception>
		public void ChangeAmountItem(StoredItem stored, int newAmount)
		{
			if (!ContainsItem(stored))
				throw new ModifingException($"Item {stored} wasn't in in backpack!");
			if (newAmount <= 0)
			{
				RemoveItem(stored);
				return;
			}

			BaseItem item = stored.Item;
			if (newAmount > item.MaxStack)
				throw new ModifingException($"newAmount {newAmount} exceedes maxStack of Item {item}");

			StoredItem oldItem = new(item, stored.Amount, stored.Position);
			stored.Amount = newAmount;
			ItemModified?.Invoke(this, oldItem, stored);
			BackpackUpdated?.Invoke(this, EventArgs.Empty);
		}

		public delegate void DItemChangedPos(object sender, Vector2Int oldPos, Vector2Int newPos);
		/// <summary>
		/// Invoked when an item position was changed.
		/// </summary>
		public event DItemChangedPos ItemChangedPosition;

		/// <summary>
		/// Moves an item to a position.
		/// </summary>
		/// <remarks>
		/// Also fires <see cref="ItemChangedPosition"/> event.
		/// </remarks>
		/// <param name="stored">The stored item.</param>
		/// <param name="to">The item where it is going to be moved.</param>
		/// <exception cref="ModifingException"></exception>
		public void MoveItem(StoredItem stored, Vector2Int to)
		{
			if (!ContainsItem(stored))
				throw new ModifingException($"Item {stored} wasn't in in backpack!");
			BaseItem item = stored.Item;

			bool occupied = AreSlotsOccupied(to, item.Size);
			if (occupied)
				throw new ModifingException($"Item {stored} couldn't be moved into occupied {to}");
			ItemChangedPosition?.Invoke(this, stored.Position, to);
			BackpackUpdated?.Invoke(this, EventArgs.Empty);
			stored.Position = to;
		}

		// Removing

		/// <summary>
		/// Invoked when an item has been removed.
		/// </summary>
		public event EventHandler<StoredItem> ItemRemoved;

		/// <summary>
		/// Thrown with errors of removing an item.
		/// </summary>
		[Serializable]
		public class RemoveException : Exception
		{
			public RemoveException() { }
			public RemoveException(string message) : base(message) { }
			public RemoveException(string message, Exception inner) : base(message, inner) { }
			protected RemoveException(
				System.Runtime.Serialization.SerializationInfo info,
				System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		/// <summary>
		/// Removes an item.
		/// </summary>
		/// <param name="stored">The item wanting the be removed.</param>
		/// <exception cref="RemoveException">If the item doesn't belong to the backpack.</exception>
		public void RemoveItem(StoredItem stored)
		{
			if (!ContainsItem(stored))
				throw new RemoveException($"Item {stored} wasn't in Backpack!");
			_StoredItems.Remove(stored);
			ItemRemoved?.Invoke(this, stored);
			BackpackUpdated?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc cref="RemoveItemAt(int, int)"/>
		/// <param name="at">The location of the slot</param>
		public StoredItem RemoveItemAt(Vector2Int at)
		{
			return RemoveItemAt(at.x, at.y);
		}

		/// <summary>
		/// Removes the item at a location.
		/// </summary>
		/// <param name="x">The x axis..</param>
		/// <param name="y">The y axis..</param>
		/// <returns>The stored item that was removed.</returns>
		/// <exception cref="RemoveException">If no item as found at the location.</exception>
		public StoredItem RemoveItemAt(int x, int y)
		{
			var stored = GetItemAt(x, y);
			if (stored == null)
				throw new RemoveException($"Item at ({x}, {y}) doesn't exist!");
			RemoveItem(stored);
			return stored;
		}

		/// <summary>
		/// Removes and drops an item from the character's inventory
		/// </summary>
		/// <param name="stored">The item stored inside of the <see cref="GridBackpack"/></param>
		/// <param name="pos">The place the item is going to be dropped at</param>
		/// <returns>The item in it's dropped state</returns>
		/// <exception cref="RemoveException">Thrown if there was an error when removing the item</exception>
		public DroppedItem DropItem(StoredItem stored, Vector3 pos)
		{
			if (!ContainsItem(stored))
				throw new RemoveException($"Item {stored} wasn't in Backpack!");
			RemoveItem(stored);
			ItemManager itemManager = ItemManager.Manager;
			var dropped = itemManager.CreateDroppedItem(stored.Item, stored.Amount, pos);
			return dropped;
		}

		/// <inheritdoc cref="DropItem(StoredItem, Vector3)"/>
		/// <param name="x">The x axis.</param>
		/// <param name="y">The y axis.</param>
		public DroppedItem DropItem(int x, int y, Vector3 pos)
		{
			var stored = GetItemAt(x, y);
			if (stored == null)
				throw new RemoveException($"Item at ({x}, {y}) doesn't exist!");
			return DropItem(stored, pos);
		}

		/// <inheritdoc cref="DropItem(StoredItem, Vector3)"/>
		/// <param name="at">The grid position</param>
		public DroppedItem DropItem(Vector2Int at, Vector3 pos)
		{
			return DropItem(at.x, at.y, pos);
		}

		// Selection

		public bool ContainsItem(StoredItem stored)
		{
			return _StoredItems.Contains(stored);
		}

		/// <inheritdoc cref="GetItemAt(int, int)"/>
		/// <param name="at">The location of the slot</param>
		public StoredItem GetItemAt(Vector2Int at)
		{
			return GetItemAt(at.x, at.y);
		}

		/// <summary>
		/// Get an item by location based on what slots it sits on.
		/// </summary>
		/// <param name="x">The x axis.</param>
		/// <param name="y">The y axis.</param>
		/// <returns>The item at the slot</returns>
		public StoredItem GetItemAt(int x, int y)
		{
			StoredItem[,] itemOccupancy = GetOccupancyWithItem();
			return itemOccupancy[x, y];
		}

		/// <summary>
		/// Converts the item list to a dictionary with the key of a <see cref="Vector2Int"/>.
		/// </summary>
		/// <returns>A Vector2Int StoredItem Dictionary.</returns>
		public Dictionary<Vector2Int, StoredItem> GetStoredItemsDict()
		{
			Dictionary<Vector2Int, StoredItem> dict = new();

			foreach (StoredItem strd in _StoredItems)
				dict.Add(strd.Position, strd);

			return dict;
		}

		// Unity

#if UNITY_EDITOR
		[ContextMenu("Add `test-item` To Backpack")]
		public void AddTestItemToFirst()
		{
			AddItem(ItemManager.Manager.GetItem("test-item"), 1);
		}
#endif
	}
}
