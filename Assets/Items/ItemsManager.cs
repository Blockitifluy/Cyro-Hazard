using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
	/// <summary>
	/// The xml path where item data is stored
	/// </summary>
	const string PathToItemsXML = "Data\\Items.xml";

	const string ItemManagerTag = "ItemManager";

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

	public struct RefItem
	{
		private readonly string _ID;
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
			readonly get
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
				var itemManager = GetManager();
				return itemManager.GetItem(_ID);
			}
		}

		public RefItem(string id, int amount)
		{
			_ID = id;

			var itemManager = GetManager();
			Item item = itemManager.GetItem(id);

			if (amount > item.MaxStack || amount < 0)
				throw new ArgumentOutOfRangeException(nameof(amount));
			_Amount = amount;
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
		/// Will the item explode when killed? 
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

	static private XmlDocument ItemsDocument;
	static private ItemManager _Manager;

	private readonly Dictionary<string, Item> Items = new();

	private Item XMLToItem(XmlElement element)
	{
		string ID = element.GetAttribute("ID");
		if (Items.ContainsKey(ID))
			return Items[ID];

		string typeString = element.SelectSingleNode("type").InnerText,
		name = element.SelectSingleNode("name").InnerText,
		description = element.SelectSingleNode("description").InnerText;

		float health = float.Parse(element.SelectSingleNode("health").InnerText),
		spoilageRate = float.Parse(element.SelectSingleNode("spoilageRate").InnerText),
		decayRate = float.Parse(element.SelectSingleNode("decayRate").InnerText),
		flamiblity = float.Parse(element.SelectSingleNode("flamiblity").InnerText),
		weight = float.Parse(element.SelectSingleNode("weight").InnerText);

		int sizeX = int.Parse(element.SelectSingleNode("sizeX").InnerText),
		sizeY = int.Parse(element.SelectSingleNode("sizeY").InnerText),
		maxStack = int.Parse(element.SelectSingleNode("maxStack").InnerText);

		bool @volatile = bool.Parse(element.SelectSingleNode("volitile").InnerText);

		Item.ItemType type;
		switch (typeString)
		{
			case "none":
				type = Item.ItemType.none;
				break;
			case "apparel":
				type = Item.ItemType.apparel;
				break;
			case "weapon":
				type = Item.ItemType.weapon;
				break;
			default:
				Debug.LogWarning($"{ID} assumed to be none type, read type {typeString}");
				type = Item.ItemType.none;
				break;
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
			Item item = XMLToItem(itemElem);
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

	public void Awake()
	{
		ItemsDocument = new XmlDocument();
		ItemsDocument.Load(PathToItemsXML);

		PreloadAllItems();
		TestForItemGetting();
	}
}