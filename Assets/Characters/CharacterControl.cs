using System.Collections.Generic;
using UnityEngine;
using CH.Items.Container;

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
		public GameObject Handle;

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

		public virtual void Awake()
		{
			MovementBasics = GetComponent<MovementBasics>();
			CharacterHealth = GetComponent<CharacterHealth>();
		}
	}
}