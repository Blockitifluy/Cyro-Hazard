using UnityEngine;
using System;

namespace CyroHazard.Items.Container
{
	/// <summary>
	/// An item inside of a <see cref="GridBackpack"/>.
	/// Including a position, amount and whatever a item has.
	/// </summary>
	public class StoredItem
	{
		/// <summary>
		/// A <see cref="ItemManager.RefItem"/> structure with item and amount. 
		/// </summary>
		private readonly IRefItem<Item> _RefItem;

		/// <summary>
		/// The position of the stored item.
		/// </summary>
		public Vector2Int Position;

		/// <summary>
		/// The item of the <see cref="StoredItem"/> 
		/// </summary>
		public Item Item
		{
			get
			{
				return _RefItem.Item;
			}
		}

		/// The amount of a certein item
		/// <summary>
		/// The amount of a certein item
		/// </summary>
		public int Amount
		{
			get
			{
				return _RefItem.Amount;
			}

			set
			{
				if (value > Item.MaxStack || value < 0)
					throw new ArgumentOutOfRangeException(nameof(value));
				_RefItem.Amount = value;
			}
		}

		public override string ToString()
		{
			return $"{Amount} {Item.ID} at {Position}";
		}

		/// <summary>
		/// Creates a stored item with a <paramref name="item"/> and <paramref name="amount"/>.
		/// </summary>
		/// <param name="item">An item</param>
		/// <param name="amount">The amount of items</param>
		public StoredItem(Item item, int amount, Vector2Int pos)
		{
			_RefItem = item.Instantiate(amount);
			Position = pos;
		}
	}
}
