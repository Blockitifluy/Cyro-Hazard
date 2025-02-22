using System;
using System.Collections.Generic;
using CH.Items.Container;
using UnityEngine;

namespace CH.Items.Interface.ActionHandler
{
    [CreateAssetMenu(fileName = "ItemActionHandler", menuName = "Items/UI/ItemActionHandler")]
    public class ItemActionHandler : ScriptableObject
    {
        public struct ActivateParams
        {
            public GameObject CommanderObject;
            public StoredItem StoredItem;
            public GridBackpack Backpack;
        }

        public abstract class ItemAction : ScriptableObject
        {
            public string Name;

            public override string ToString()
            {
                return Name;
            }

            public abstract void Activate(ActivateParams activate);
        }

        public virtual Type GetTargetType()
        {
            return typeof(Item);
        }

        public List<ItemAction> ItemActions;
    }
}