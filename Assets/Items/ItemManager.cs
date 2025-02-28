using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CH.Items
{
	[Serializable]
	public struct RefItem
	{
		private readonly string _ID;
		[SerializeField]
		private int _Amount;

		public readonly string ID
		{
			get
			{
				return _ID;
			}
		}

		public int Amount
		{
			get
			{
				return _Amount;
			}

			set
			{
				if (_Amount > value || _Amount <= 0)
					throw new ArgumentOutOfRangeException(nameof(value));
				_Amount = value;
			}
		}

		public readonly Item Item
		{
			get
			{
				var itemManager = ItemManager.GetManager();
				return itemManager.GetItem(_ID);
			}
		}

		public RefItem(string id, int amount)
		{
			_ID = id;

			var itemManager = ItemManager.GetManager();
			Item item = itemManager.GetItem(id);

			if (0 < amount || amount <= item.MaxStack)
				_Amount = amount;
			else
				throw new ArgumentOutOfRangeException(nameof(amount));
		}
	}

	/// <summary>
	/// Non-contrete class of item data
	/// </summary>
	[Serializable]
	public class Item
	{
		/// <summary>
		/// <inheritdoc cref="Type"/>
		/// </summary>
		public enum ItemType : ushort
		{
			none, weapon, apparel
		}

		/// <summary>
		/// The ID of the item
		/// </summary>
		public readonly string ID;

		/// <summary>
		/// The type of item it is. Can be:
		/// <list type="bullet">
		/// <item><c>none</c>,</item>
		/// <item><c>weapon</c>,</item>
		/// <item><c>apparel</c></item>
		/// </list>
		/// </summary>
		public readonly ItemType Type;

		/// <summary>
		/// The displayed name of the item
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The long, long description of the item.
		/// </summary>
		public readonly string Description;

		/// <summary>
		/// The maximium amount of health the item can have
		/// </summary>
		public readonly float MaxHealth;

		/// <summary>
		/// How much the item spoils per day, when not kept in the right conditions.
		/// </summary>
		public readonly float SpoilageRate;

		/// <summary>
		/// How much the item decays per day, when not kept in the right conditions.
		/// </summary>
		public readonly float DecayRate;

		/// <summary>
		/// How flamible the item is.
		/// </summary>
		public readonly float Flamiblity;

		/// <summary>
		/// Will the item explode when killed 
		/// </summary>
		public readonly bool Volitile;

		/// <summary>
		/// The size of item inside of a <seealso cref="ItemStorage"/>
		/// </summary>
		public readonly Vector2Int Size;

		/// <summary>
		/// The maximum stack of item
		/// </summary>
		public readonly int MaxStack;

		/// <summary>
		/// The weight in kg per item
		/// </summary>
		public readonly float Weight;

		public override string ToString()
		{
			return $"{Name} ({ID})";
		}

		internal Item(string id, string name, float maxHealth,
		float spoilageRate, float decayRate, float flamiblity,
		bool @volatile, string description, ItemType type,
		int maxStack, Vector2Int size, float weight
		)
		{
			ID = id;
			Name = name;
			MaxHealth = maxHealth;
			SpoilageRate = spoilageRate;
			DecayRate = decayRate;
			Flamiblity = flamiblity;
			Volitile = @volatile;
			Description = description;
			Type = type;
			MaxStack = maxStack;
			Size = size;
			Weight = weight;
		}
	}

	public class ItemManager : MonoBehaviour
	{
		/// <summary>
		/// The xml path where item data is stored
		/// </summary>
		const string PathToItemsXML = "Data\\Items\\Items.xml";

		const string ItemManagerTag = "ItemManager";

		public GameObject DroppedPrefab;

		/// <summary>
		/// Thrown if a item can't be found
		/// </summary>
		[Serializable]
		public class ItemNotFoundException : ArgumentException
		{
			public ItemNotFoundException() { }
			public ItemNotFoundException(string message) : base(message) { }
			public ItemNotFoundException(string message, Exception inner) : base(message, inner) { }
			protected ItemNotFoundException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		static private XmlDocument ItemsDocument;
		static private ItemManager _Manager;

		private readonly Dictionary<string, Item> Items = new();

		private Item ConvertXMLToItem(XmlElement element)
		{
			string ID = element.GetAttribute("ID"),
			typeString = element.GetNodeText("type"),
			name = element.GetNodeText("name"),
			description = element.GetNodeText("description");

			float health = float.Parse(element.GetNodeText("health")),
			spoilageRate = float.Parse(element.GetNodeText("spoilageRate")),
			decayRate = float.Parse(element.GetNodeText("decayRate")),
			flamiblity = float.Parse(element.GetNodeText("flamiblity")),
			weight = float.Parse(element.GetNodeText("weight"));

			int sizeX = int.Parse(element.GetNodeText("sizeX")),
			sizeY = int.Parse(element.GetNodeText("sizeY")),
			maxStack = int.Parse(element.GetNodeText("maxStack"));

			bool @volatile = bool.Parse(element.GetNodeText("volitile"));

			if (!Enum.TryParse<Item.ItemType>(typeString, out var type))
			{
				Debug.LogWarning($"{ID} assumed to be none type, read type {typeString}");
				type = Item.ItemType.none;
			}

			Vector2Int size = new(sizeX, sizeY);
			Item item = new(
			  ID, name, health, spoilageRate,
			  decayRate, flamiblity, @volatile,
			  description, type, maxStack, size,
			  weight
			);

			return item;
		}

		static public ItemManager GetManager()
		{
			if (_Manager != null)
				return _Manager;
			var obj = GameObject.FindGameObjectWithTag(ItemManagerTag);
			ItemManager itemManager = obj.GetComponent<ItemManager>();
			_Manager = itemManager;
			return itemManager;
		}

		/// <summary>
		/// Get's an item by it's <paramref name="ID"/>
		/// </summary>
		/// <param name="ID">The ID of the item</param>
		/// <returns>An item with the same ID</returns>
		/// <exception cref="NullReferenceException">Thrown if the item couldn't be found</exception>
		public Item GetItem(string ID)
		{
			Item item = Items[ID];
			if (item == null)
				throw new NullReferenceException($"Item ({ID}) couldn't be found");
			return item;
		}

		private void PreloadAllItems()
		{
			var itemElements = ItemsDocument.DocumentElement.ChildNodes;
			int loaded = 0;

			foreach (XmlElement itemElem in itemElements)
			{
				Item item = ConvertXMLToItem(itemElem);
				Items.Add(item.ID, item);
				loaded++;
			}
			Debug.Log($"Loaded all {loaded} item(s)");
		}

		private void TestForItemGetting()
		{
			Debug.Log("Trying to get test-item for testing purposes");

			Item itm;

			try
			{
				itm = GetItem("test-item");
			}
			catch (NullReferenceException)
			{
				Debug.LogError("test-item was null");
				Application.Quit(1);
			}

			Debug.Log("SUCCESS!");
			return;
		}

		public DroppedItem CreateDroppedItem(Item item, int amount, Vector3 pos)
		{
			GameObject obj = Instantiate(DroppedPrefab, null, false);

			obj.SetActive(false); // Unity thing or whatever

			DroppedItem dropped = obj.GetComponent<DroppedItem>();
			dropped.RefItem = new(item.ID, amount);
			dropped._Health = item.MaxHealth;

			obj.transform.SetParent(null);
			obj.transform.position = pos;

			obj.SetActive(true);

			return dropped;
		}

#if UNITY_EDITOR
		[ContextMenu("Print All Item Names")]
		public void PrintAllItemName()
		{
			List<string> names = new();
			foreach (string name in Items.Keys)
			{
				names.Add(name);
			}

			Debug.Log(string.Join(", ", names));
		}
#endif

		public void Awake()
		{
			ItemsDocument = new XmlDocument();
			ItemsDocument.Load(Application.streamingAssetsPath + "\\" + PathToItemsXML);

			PreloadAllItems();
			TestForItemGetting();
		}
	}
}
