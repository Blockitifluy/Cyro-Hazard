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
	/// The interface version of <see cref="RefItem"/>.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="RefItem<TItem>" path="/summary"/>
	/// </remarks>
	/// <typeparam name="TItem">The item</typeparam>
	public interface IRefItem<out TItem> where TItem : BaseItem
	{
		public string ID { get; }

		public int Amount { get; set; }

		public TItem Item { get; }
	}

	/// <summary>
	/// Refrences a <see cref="BaseItem"/> that is instanced.
	/// Has an amount and other propetries.
	/// </summary>
	/// <typeparam name="TItem">The item being instanced.</typeparam>
	[Serializable]
	public class RefItem<TItem> : IRefItem<TItem> where TItem : BaseItem
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
				var itemManager = ItemManager.Manager;
				return itemManager.GetItem<TItem>(_ID);
			}
		}

		public RefItem(string id, int amount)
		{
			_ID = id;

			var itemManager = ItemManager.Manager;
			TItem item = itemManager.GetItem<TItem>(id);

			if (0 < amount || amount <= item.MaxStack)
				_Amount = amount;
			else
				throw new ArgumentOutOfRangeException(nameof(amount));
		}

		public RefItem(TItem item, int amount) : this(item.ID, amount) { }
	}

	/// <summary>
	/// Mark this mark this method as item, to be loaded.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ItemAttribute : Attribute
	{
		public readonly string XMLName;

		public ItemAttribute(string xmlName)
		{
			XMLName = xmlName;
		}
	}

	///	<summary>
	///	Marks a method inside a Item with a name, then be able
	/// to be called when right clicking a <see cref="StoredItem"/>.
	/// </summary>
	/// <remarks>
	/// For a ItemAction be discoverable and valid, add the <see cref="ItemActionAttribute"/> to it
	/// , and the return should be null and params be <see cref="ActivateParams"/>.
	/// </remarks>
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
	/// The base class of all item types.
	/// </summary>
	public abstract class BaseItem
	{
		/// <summary>
		/// Parameters for <see cref="DItemAction"/>.
		/// </summary>
		public struct ItemActionParams
		{
			public GameObject CallerObject;
			public StoredItem StoredItem;
			public GridBackpack Backpack;
		}

		/// <summary>
		/// A delegate of a <see cref="ItemActionAttribute"/>.
		/// </summary>
		/// <param name="params">
		/// The parameters of <see cref="DItemAction"/> used by the Backpack UI.
		/// 
		/// <inheritdoc cref="ActionMenuItem" path="/summary"/>
		///	</param>
		public delegate void DItemAction(ItemActionParams @params);

		/// <summary>
		/// Represents a menu action in <see cref="Interface.BackpackUI"/>,
		/// with a name and a <see cref="DItemAction"/> delegate.
		/// </summary>
		public struct ActionMenuItem
		{
			/// <summary>
			/// The name of the <see cref="ActionMenuItem"/>
			/// </summary>
			public string Name;
			/// <inheritdoc cref="DItemAction"/>
			public DItemAction ItemAction;
		}

		// Public Methods

		/// <summary>
		/// Gets and returns all item actions, with names and delegate.
		/// </summary>
		/// <remarks>
		/// <inheritdoc cref="ItemActionAttribute" path="/remark"/>
		/// </remarks>
		/// <returns>All of the found Item Actions.</returns>
		public IEnumerable<ActionMenuItem> GetItemActions()
		{
			MethodInfo[] methods = GetType().GetMethods();

			foreach (MethodInfo method in methods)
			{
				var actionAttr = method.GetCustomAttribute<ItemActionAttribute>();
				if (actionAttr is null) continue;

				string name = actionAttr.ActionName;
				DItemAction itemAction = GetActionFromMethodInfo(method);

				yield return new()
				{
					Name = name,
					ItemAction = itemAction
				};
			}
		}

		[ItemAction("Drop")]
		public void DropAction(ItemActionParams @params)
		{
			var backpack = @params.Backpack;
			var pos = @params.CallerObject.transform.position;
			backpack.DropItem(@params.StoredItem, pos);
		}

		// Private Methods

		private DItemAction GetActionFromMethodInfo(MethodInfo method)
		{
			try
			{
				DItemAction action = (DItemAction)method.CreateDelegate(typeof(DItemAction), this);
				return action;
			}
			catch (Exception)
			{
				Debug.LogWarning($"{method.Name} is marked with {nameof(ItemActionAttribute)} but doesn't follow the return type or params of {nameof(DItemAction)}.");
				return null;
			}
		}

		#region ItemPropetries

		/// <summary>
		/// The ID of the item
		/// </summary>
		[XmlAttribute("ID")]
		public string ID { get; set; }

		/// <summary>
		/// The displayed name of the item
		/// </summary>
		[XmlElement("name")]
		public string Name { get; set; }

		/// <summary>
		/// The long, long description of the item.
		/// </summary>
		[XmlElement("description")]
		public string Description { get; set; }

		/// <summary>
		/// The maximium amount of health the item can have
		/// </summary>
		[XmlElement("health")]
		public float MaxHealth { get; set; }

		/// <summary>
		/// How much the item spoils per day, when not kept in the right conditions.
		/// </summary>
		[XmlElement("spoilage-rate")]
		public float SpoilageRate { get; set; }

		/// <summary>
		/// How much the item decays per day, when not kept in the right conditions.
		/// </summary>
		[XmlElement("decay-rate")]
		public float DecayRate { get; set; }

		/// <summary>
		/// How flamible the item is.
		/// </summary>
		[XmlElement("flamibility")]
		public float Flamiblity { get; set; }

		/// <summary>
		/// Will the item explode when killed 
		/// </summary>
		[XmlElement("flamiblity")]
		public bool Volitile { get; set; }

		[XmlElement("size-x")]
		public int SizeX { get; set; }

		[XmlElement("size-y")]
		public int SizeY { get; set; }

		/// <summary>
		/// The size of item inside of a <seealso cref="ItemStorage"/>
		/// </summary>
		public Vector2Int Size => new(SizeX, SizeY);

		/// <summary>
		/// The maximum stack of item
		/// </summary>
		[XmlElement("max-stack")]
		public int MaxStack { get; set; }

		/// <summary>
		/// The weight in kg per item
		/// </summary>
		[XmlElement("weight")]
		public float Weight { get; set; }

		#endregion

		/// <summary>
		/// Creates a RefItem based on the item.
		/// </summary>
		/// <param name="amount">The amount of item supplied.</param>
		/// <returns>The reference item.</returns>
		public virtual IRefItem<BaseItem> Instantiate(int amount)
		{
			RefItem<BaseItem> refItem = new(this, amount);

			return refItem;
		}
	}

	/// <summary>
	/// Incharge of revieving, loading and dropping items. 
	/// </summary>
	[AddComponentMenu("Items/Item Manager")]
	public class ItemManager : MonoBehaviour
	{
		/// <summary>
		/// The xml path where item data is stored
		/// </summary>
		const string PathToItemsXML = "Data\\Items\\Items.xml";

		const string ItemManagerTag = "ItemManager";

		public GameObject DroppedPrefab;
		public GameObject ToolPrefab;

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

		static public ItemManager Manager
		{
			get
			{
				if (_Manager != null)
					return _Manager;
				var obj = GameObject.FindGameObjectWithTag(ItemManagerTag);
				ItemManager itemManager = obj.GetComponent<ItemManager>();
				_Manager = itemManager;
				return itemManager;
			}
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
