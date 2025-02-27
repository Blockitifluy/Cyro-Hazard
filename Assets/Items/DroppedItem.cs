using System;
using CH.Items.Container;
using UnityEngine;

namespace CH.Items
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Items/Dropped Item")]
	public class DroppedItem : MonoBehaviour
	{
		/// <summary>
		/// The item manager, hju!
		/// </summary>
		private ItemManager _ItemsManager;

		[SerializeField]
		/// <summary>
		/// How much health does the item have.
		/// When it has 0 health it disappears.
		/// </summary>
		internal float _Health = 100.0f;

		[SerializeField]
		internal RefItem RefItem;

		/// <inheritdoc cref="_Item"/>
		public Item Item
		{
			get { return RefItem.Item; }
			set { RefItem = new(value.ID, Amount); }
		}

		/// <inheritdoc cref="_Health"/>
		public float Health
		{
			get { return _Health; }
			set { _Health = Mathf.Clamp(value, 0, Item.MaxHealth); }
		}

		/// <inheritdoc cref="_Amount"/>
		public int Amount
		{
			get { return RefItem.Amount; }
			set { RefItem.Amount = Mathf.Clamp(value, 1, Item.MaxStack); }
		}

		public string ID;

		/// <summary>
		/// Pickups up the dropped item, then destroying it.
		/// </summary>
		/// <param name="backpack">The backpack to be inserted into.</param>
		/// <returns>The pickup represented as a stored item </returns>
		public StoredItem PickupDropped(GridBackpack backpack)
		{
			StoredItem stored;

			try
			{
				stored = backpack.AddItem(Item, Amount);
			}
			catch (GridBackpack.ModifingException)
			{
				Debug.Log($"There was no place to add {Item} to {backpack}");
				throw;
			}

			Destroy(gameObject);
			return stored;
		}

		public void Start()
		{
			_ItemsManager = ItemManager.GetManager();
			RefItem = new(ID, Amount);
		}

		// void OnEnable() => Start();

		public void LateUpdate()
		{
			if (Health <= 0)
			{
				Destroy(this);
			}
		}
	}
}
