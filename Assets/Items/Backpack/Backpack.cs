using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
	private readonly List<StoredItem> _StoredItems = new();
	private ItemManager _ItemManager;

	public Vector2Int Size;

	public bool[,] GetOccupancy()
	{
		bool[,] occupancy = new bool[Size.x, Size.y];

		foreach (StoredItem strd in _StoredItems)
		{
			ItemManager.Item Item = strd.Item;
			int area = Item.Size.x * Item.Size.y;

			for (int i = 0; i < area; i++)
			{
				int x = i % Item.Size.x,
				y = i / Item.Size.y;
				occupancy[x, y] = true;
			}
		}

		return occupancy;
	}

	public StoredItem GetItemAt(Vector2Int at)
	{
		return GetStoredItemsDict()[at];
	}

	public bool IsSlotOccupied(int x, int y)
	{
		// TODO - use long values for quicker bitwise operations
		bool[,] occupancy = GetOccupancy();

		bool inBoundX = 0 < x || x <= Size.x,
		inBoundY = 0 < y || y < Size.y;

		if (!inBoundX)
			throw new ArgumentOutOfRangeException(nameof(x));
		if (!inBoundY)
			throw new ArgumentOutOfRangeException(nameof(y));

		return occupancy[x, y];
	}

	public bool IsSlotOccupied(Vector2Int at)
	{
		return IsSlotOccupied(at.x, at.y);
	}

	public bool AreSlotsOccupied(Vector2Int at, Vector2Int size)
	{
		// TODO - size and at can be out of range with no error
		bool[,] occupancy = GetOccupancy();
		int area = size.x * size.y;

		for (int i = 0; i < area; i++)
		{
			int x = i % size.x,
			y = i / size.x;

			int Ax = at.x + x,
			Ay = at.y + y;

			if (occupancy[Ax, Ay])
				return false;
		}

		return true;
	}

	public StoredItem AddItemAt(ItemManager.Item item, int amount, Vector2Int at)
	{
		if (AreSlotsOccupied(at, item.Size))
			throw new ArgumentOutOfRangeException(nameof(at));

		StoredItem storedItem = new(item, amount, at);

		return storedItem;
	}

	public Dictionary<Vector2Int, StoredItem> GetStoredItemsDict()
	{
		Dictionary<Vector2Int, StoredItem> dict = new();

		foreach (StoredItem strd in _StoredItems)
			dict.Add(strd.Position, strd);

		return dict;
	}

	public void Start()
	{
		_ItemManager = ItemManager.GetManager();
	}
}
