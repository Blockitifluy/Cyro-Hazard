using System;
using CyroHazard.Character;
using CyroHazard.Items.Container;
using CyroHazard.Items.ItemVariants;
using UnityEngine;

namespace CyroHazard.Tools
{
    public class BaseTool : MonoBehaviour
    {
        public CharacterControl Controller => _Controller;
        public StoredItem StoredItem => _StoredItem;
        public GridBackpack Backpack => _Backpack;
        public ToolItem ToolItem;

        private StoredItem _StoredItem;
        private CharacterControl _Controller;
        private GridBackpack _Backpack;

        public virtual void Equip(StoredItem stored, CharacterControl controller)
        {
            bool hasItem = controller.FindItemInBackpacks(stored, out var backpack);

            if (!hasItem)
                throw new NullReferenceException($"Item {stored} wasn't in any Backpack!");
            if (stored.Item is not ToolItem toolItem)
                throw new InvalidCastException($"The item type doesn't derive from {StoredItem.Item.GetType().FullName}");

            _StoredItem = stored;
            _Controller = controller;
            _Backpack = backpack;
            ToolItem = toolItem;

            Backpack.RemoveItem(stored);
        }

        public virtual void Unequip()
        {
            Backpack.AddItem(_StoredItem, true);
            _StoredItem = null;
            Destroy(gameObject);
        }
    }
}