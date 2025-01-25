using UnityEngine;

namespace CH.Items
{
	public class DroppedItems : MonoBehaviour
	{
		/// <summary>
		/// The item manager, hju!
		/// </summary>
		private ItemManager _ItemsManager;
		/// <summary>
		/// The item that the <see cref="DroppedItem"/> references.
		/// </summary>
		private ItemManager.Item _Item;
		/// <summary>
		/// How much health does the item have.
		/// When it has 0 health it disappears.
		/// </summary>
		private float _Health;

		/// <inheritdoc cref="_Item"/>
		public ItemManager.Item Item
		{
			get { return _Item; }
		}

		/// <inheritdoc cref="_Health"/>
		public float Health
		{
			get { return _Health; }
			set { _Health = Mathf.Clamp(value, 0, Item.MaxHealth); }
		}

		public string ID;

		// Start is called before the first frame update
		public void Start()
		{
			_ItemsManager = ItemManager.GetManager();
			_Item = _ItemsManager.GetItem(ID);
		}

		// Update is called once per frame
		public void Update()
		{
			if (Health <= 0)
			{
				Destroy(this);
			}
		}
	}
}
