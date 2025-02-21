using System;
using UnityEngine;

// TODO - Add pickup

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
		/// <summary>
		/// The item that the <see cref="DroppedItem"/> references.
		/// </summary>
		private Item _Item;
		/// <summary>
		/// How much health does the item have.
		/// When it has 0 health it disappears.
		/// </summary>
		internal float _Health;
		/// <summary>
		/// How many stacks does this <see cref="DroppedItem"/> contain? 
		/// </summary>
		internal int _Amount = 1;

		/// <inheritdoc cref="_Item"/>
		public Item Item
		{
			get { return _Item; }
			set { _Item = value; }
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
			get { return _Amount; }
			set { _Amount = Mathf.Clamp(_Amount, 1, _Item.MaxStack); }
		}

		public string ID;

		public void Awake()
		{
			_ItemsManager = ItemManager.GetManager();
			try
			{
				_Item = _ItemsManager.GetItem(ID);
			}
			catch (Exception) { }
		}

		void OnEnable() => Awake();

		public void LateUpdate()
		{
			if (Health <= 0)
			{
				Destroy(this);
			}
		}
	}
}
