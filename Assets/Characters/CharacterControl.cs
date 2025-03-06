using System.Collections.Generic;
using UnityEngine;
using CH.Items.Container;
using CH.Tools;
using System;
using CH.Items.ItemVariants;

namespace CH.Character
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
			return (TTool)_Tool;
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
		}

		public void Equip(StoredItem stored, bool autoUnequip = false)
		{
			if (stored.Item is not ToolItem)
				throw new InvalidCastException($"");

			if (autoUnequip)
				UnequipCurrentTool(throwIfNoTool: false);

			BaseTool tool = ToolItem.CreateTool<ToolItem>();
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

		public List<GridBackpack> DetectBackpacks()
		{
			var allBackpacks = GameObject.FindGameObjectsWithTag("Backpack");
			List<GridBackpack> actualPacks = new();

			foreach (GameObject obj in allBackpacks)
			{
				if (gameObject.transform.IsChildOf(obj.transform)) continue;

				if (!obj.TryGetComponent<GridBackpack>(out var backpack))
				{
					Debug.LogWarning($"Even though {obj.name} has tag backpack, it's doesn't have the Backpack component");
					continue;
				}

				actualPacks.Add(backpack);
			}

			return actualPacks;
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