using System;
using CyroHazard.Character;
using CyroHazard.Items.Container;
using CyroHazard.Items.ItemVariants;
using UnityEngine;

namespace CyroHazard.Tools
{
    public class BaseTool : MonoBehaviour
    {
        /// <summary>
        /// The character that the tool is equipped with.
        /// </summary>
        public CharacterControl Controller => _Controller;
        /// <inheritdoc cref="Controller"/>
        private CharacterControl _Controller;

        /// <summary>
        /// The stored version of the tool.
        /// </summary>
        public StoredItem StoredItem => _StoredItem;
        /// <inheritdoc cref="StoredItem"/>
        private StoredItem _StoredItem;

        /// <summary>
        /// The backpack the tool is stored in.
        /// </summary>
        public GridBackpack Backpack => _Backpack;
        /// <inheritdoc cref="Backpack"/>
        private GridBackpack _Backpack;

        public BaseToolItem ToolItem;

        public virtual void Equip(StoredItem stored, CharacterControl controller)
        {
            bool hasItem = controller.FindItemInBackpacks(stored, out var backpack);

            if (!hasItem)
                throw new NullReferenceException($"Item {stored} wasn't in any Backpack!");
            if (stored.Item is not BaseToolItem toolItem)
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