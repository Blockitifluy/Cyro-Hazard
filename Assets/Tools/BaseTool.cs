using System;
using CH.Character;
using CH.Items.Container;
using UnityEngine;

namespace CH.Tools
{
    public abstract class BaseTool : MonoBehaviour
    {
        public CharacterControl Controller => _Controller;
        public StoredItem LinkedItem => _LinkedItem;
        public GridBackpack Backpack => _Backpack;

        private StoredItem _LinkedItem;
        private CharacterControl _Controller;
        private GridBackpack _Backpack;

        public virtual void Equip(StoredItem stored, CharacterControl controller)
        {
            bool hasItem = controller.FindItemInBackpacks(stored, out var backpack);

            if (!hasItem)
                throw new NullReferenceException($"Item {stored} wasn't in any Backpack!");

            _LinkedItem = stored;
            _Controller = controller;
            _Backpack = backpack;

            Backpack.RemoveItem(stored);
        }

        public virtual void Unequip()
        {
            Backpack.AddItem(_LinkedItem, true);
            _LinkedItem = null;
        }
    }
}