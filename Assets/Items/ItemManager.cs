using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using CH.Items.Container;
using System.Reflection;

namespace CH.Items
{
	/// <summary>
	/// Refrences a <see cref="BaseItem"/> that is instanced.
	/// Has an amount and other propetries.
	/// </summary>
	/// <typeparam name="TItem">The item being instanced.</typeparam>
	[Serializable]
	public class RefItem<TItem> where TItem : BaseItem
	{
		[SerializeField]
		private string _ID;
		[SerializeField]
		private int _Amount;

		public string ID => _ID;

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

		public TItem Item
		{
			get
			{
				var itemManager = ItemManager.GetManager();
				return itemManager.GetItem<TItem>(_ID);
			}
		}

		public RefItem(string id, int amount)
		{
			_ID = id;

			var itemManager = ItemManager.GetManager();
			TItem item = itemManager.GetItem<TItem>(id);

			if (0 < amount || amount <= item.MaxStack)
				_Amount = amount;
			else
				throw new ArgumentOutOfRangeException(nameof(amount));
		}

		public RefItem(TItem item, int amount) : this(item.ID, amount) { }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ItemAttribute : Attribute
	{
		public readonly string XMLName;

		public ItemAttribute(string xmlName)
		{
			XMLName = xmlName;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ItemActionAttribute : Attribute
	{
		public readonly string ActionName;

		public ItemActionAttribute(string actionName)
		{
			ActionName = actionName;
		}
	}

	/// <summary>
	/// The interface of all item types.
	/// </summary>
	public abstract class BaseItem
	{
		public struct ActivateParams
		{
			public GameObject CommanderObject;
			public StoredItem StoredItem;
			public GridBackpack Backpack;
		}

		public delegate void ItemAction(ActivateParams activate);

		public struct ActionMenuItem
		{
			public string Name;
			public ItemAction ItemAction;
		}


		public IEnumerable<ActionMenuItem> GetItemActions()
		{
			MethodInfo[] methods = GetType().GetMethods();

			foreach (MethodInfo method in methods)
			{
				var actionAttr = (ItemActionAttribute)method.GetCustomAttribute(typeof(ItemActionAttribute));
				if (actionAttr is null) continue;

				string name = actionAttr.ActionName;
				ItemAction action = (ItemAction)method.CreateDelegate(typeof(ItemAction), this);

				yield return new()
				{
					Name = name,
					ItemAction = action
				};
			}
		}

		[ItemAction("Drop")] // TODO - Implement
		public void DropAction(ActivateParams activate)
		{
			var backpack = activate.Backpack;
			var pos = activate.CommanderObject.transform.position;
			backpack.DropItem(activate.StoredItem, pos);
		}

		[XmlAttribute("ID")]
		/// <summary>
		/// The ID of the item
		/// </summary>
		public string ID { get; set; }

		[XmlElement("name")]
		/// <summary>
		/// The displayed name of the item
		/// </summary>
		public string Name { get; set; }

		[XmlElement("description")]
		/// <summary>
		/// The long, long description of the item.
		/// </summary>
		public string Description { get; set; }

		[XmlElement("health")]
		/// <summary>
		/// The maximium amount of health the item can have
		/// </summary>
		public float MaxHealth { get; set; }

		[XmlElement("spoilage-rate")]
		/// <summary>
		/// How much the item spoils per day, when not kept in the right conditions.
		/// </summary>
		public float SpoilageRate { get; set; }

		[XmlElement("decay-rate")]
		/// <summary>
		/// How much the item decays per day, when not kept in the right conditions.
		/// </summary>
		public float DecayRate { get; set; }

		[XmlElement("flamibility")]
		/// <summary>
		/// How flamible the item is.
		/// </summary>
		public float Flamiblity { get; set; }

		[XmlElement("flamiblity")]
		/// <summary>
		/// Will the item explode when killed 
		/// </summary>
		public bool Volitile { get; set; }

		[XmlElement("size-x")]
		public int SizeX { get; set; }

		[XmlElement("size-y")]
		public int SizeY { get; set; }

		/// <summary>
		/// The size of item inside of a <seealso cref="ItemStorage"/>
		/// </summary>
		public Vector2Int Size => new(SizeX, SizeY);

		[XmlElement("max-stack")]
		/// <summary>
		/// The maximum stack of item
		/// </summary>
		public int MaxStack { get; set; }

		[XmlElement("weight")]
		/// <summary>
		/// The weight in kg per item
		/// </summary>
		public float Weight { get; set; }

		public virtual RefItem<BaseItem> Instaniate(int amount)
		{
			RefItem<BaseItem> refItem = new(this, amount);

			return refItem;
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

		private readonly Dictionary<string, BaseItem> Items = new();
		private readonly Dictionary<string, Type> ItemTypes = new();

		private BaseItem ConvertXMLToItem(XmlElement element)
		{
			Type itemType = default;

			foreach (KeyValuePair<string, Type> pair in ItemTypes)
			{
				if (pair.Key != element.Name)
					continue;
				itemType = pair.Value;
			}

			XmlSerializer serializer = new(itemType);

			using StringReader reader = new(element.OuterXml);
			BaseItem item = (BaseItem)serializer.Deserialize(reader);

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
		public BaseItem GetItem(string ID) => GetItem<BaseItem>(ID);

		/// <inheritdoc cref="GetItem(string)"/>
		/// <typeparam name="TItem">The type of Item being retrieved</typeparam>
		public TItem GetItem<TItem>(string ID) where TItem : BaseItem
		{
			if (Items[ID] is not TItem item)
				throw new NullReferenceException($"Item ({ID}) couldn't be found");
			return item;
		}

		private void LoadAllItemTypes()
		{
			var itemTypes = Helper.GetTypesWithAttribute<ItemAttribute>(GetType().Assembly);

			foreach (Type tp in itemTypes)
			{
				ItemAttribute itemAtt = (ItemAttribute)Attribute.GetCustomAttribute(tp, typeof(ItemAttribute));
				ItemTypes.Add(itemAtt.XMLName, tp);
			}
		}

		private void PreloadAllItems()
		{
			var itemElements = ItemsDocument.DocumentElement.ChildNodes;
			int loaded = 0;

			foreach (XmlElement itemElem in itemElements)
			{
				BaseItem item = ConvertXMLToItem(itemElem);

				Items.Add(item.ID, item);
				loaded++;
			}
			Debug.Log($"Loaded all {loaded} item(s)");
		}

		private void TestForItemGetting()
		{
			Debug.Log("Trying to get test-item for testing purposes");

			BaseItem itm;

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

		public DroppedItem CreateDroppedItem(BaseItem item, int amount, Vector3 pos)
		{
			GameObject obj = Instantiate(DroppedPrefab, null, false);
			obj.transform.SetParent(null);
			obj.transform.position = pos;

			DroppedItem dropped = obj.GetComponent<DroppedItem>();
			dropped.RefItem = new(item.ID, amount);
			dropped._Health = item.MaxHealth;

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

			LoadAllItemTypes();
			PreloadAllItems();
			TestForItemGetting();
		}
	}
}
