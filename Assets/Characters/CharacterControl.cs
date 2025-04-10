using System.Collections.Generic;
using UnityEngine;
using CyroHazard.Items.Container;
using CyroHazard.Tools;
using System;
using CyroHazard.Items.ItemVariants;
using CyroHazard.Items;

namespace CyroHazard.Character
{
	[RequireComponent(typeof(MovementBasics))]
	[RequireComponent(typeof(CharacterHealth))]
	public abstract class CharacterControl : MonoBehaviour
	{

		[HideInInspector]
		public MovementBasics MovementBasics;
		[HideInInspector]
		public CharacterHealth CharacterHealth;
		public Transform Handle;

		protected BaseTool _Tool;

		public TTool GetTool<TTool>() where TTool : BaseTool
		{
			return _Tool as TTool;
		}

		public BaseTool GetTool()
		{
			return GetTool<BaseTool>();
		}

		public void UnequipCurrentTool(bool throwIfNoTool = true)
		{
			if (_Tool == null)
			{
				if (!throwIfNoTool)
					return;
				throw new NullReferenceException("Tried to unequip, No Tool Found!");
			}

			_Tool.Unequip();
			_Tool = null;
		}

		public void Equip(StoredItem stored, bool autoUnequip = false)
		{
			if (stored.Item is not BaseToolItem item)
				throw new InvalidCastException($"The item isn't a tool item!");

			if (autoUnequip)
				UnequipCurrentTool(throwIfNoTool: false);

			BaseTool tool = item.CreateTool();
			tool.Equip(stored, this);
			tool.transform.SetParent(Handle, worldPositionStays: false);

			_Tool = tool;
		}

		public bool FindItemInBackpacks(StoredItem stored, out GridBackpack backpack)
		{
			var backpacks = DetectBackpacks();

			foreach (GridBackpack search in backpacks)
			{
				bool contains = search.ContainsItem(stored);
				if (!contains)
					continue;

				backpack = search;
				return true;
			}

			backpack = null;
			return false;
		}

		public IEnumerable<GridBackpack> DetectBackpacks()
		{
			var allBackpacks = GameObject.FindGameObjectsWithTag("Backpack");

			foreach (GameObject obj in allBackpacks)
			{
				if (gameObject.transform.IsChildOf(obj.transform)) continue;

				if (!obj.TryGetComponent<GridBackpack>(out var backpack))
				{
					Debug.LogWarning($"Even though {obj.name} has tag backpack, it's doesn't have the Backpack component");
					continue;
				}

				yield return backpack;
			}
		}

		public GridBackpack GetFirstBackpack()
		{
			var allBackpacks = GameObject.FindGameObjectsWithTag("Backpack");

			foreach (GameObject obj in allBackpacks)
			{
				if (gameObject.transform.IsChildOf(obj.transform)) continue;

				if (!obj.TryGetComponent<GridBackpack>(out var backpack))
				{
					Debug.LogWarning($"Even though {obj.name} has tag backpack, it's doesn't have the Backpack component");
					continue;
				}

				return backpack;
			}

			return null;
		}

		public abstract Vector3 GetAimDirection();

		/// <inheritdoc cref="TryToAddItemToBackpacks(StoredItem)"/>
		/// <param name="item">The item to be added.</param>
		/// <param name="amount">The amount item added.</param>
		/// <returns>If adding the item was successful.</returns>
		public bool TryToAddItemToBackpacks(Item item, int amount, out StoredItem storedItem)
		{
			var backpacks = DetectBackpacks();

			foreach (GridBackpack pack in backpacks)
			{
				bool successul = pack.CanFindPlacementFor(item, out var pos);

				if (!successul)
					continue;

				storedItem = pack.AddItemAt(item, amount, pos);
				return true;
			}

			storedItem = null;
			return false;
		}

		/// <summary>
		/// Trys to add an item to all backpacks that the character has access to.
		/// </summary>
		/// <param name="storedItem">StoredItem</param>
		public void TryToAddItemToBackpacks(StoredItem storedItem)
		{
			var backpacks = DetectBackpacks();

			foreach (GridBackpack pack in backpacks)
			{
				bool successul = pack.CanFindPlacementFor(storedItem.Item, out var pos);

				if (!successul)
					continue;

				pack.AddItemAt(storedItem, pos);
				return;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Unequip Current Tool")]
		public void TestUnequip() => UnequipCurrentTool();
#endif

		public virtual void Awake()
		{
			MovementBasics = GetComponent<MovementBasics>();
			CharacterHealth = GetComponent<CharacterHealth>();
		}
	}
}