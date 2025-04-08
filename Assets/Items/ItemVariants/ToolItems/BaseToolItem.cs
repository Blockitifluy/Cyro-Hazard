using System.Xml.Serialization;
using CyroHazard.Character.Player;
using CyroHazard.Tools;
using UnityEngine;

namespace CyroHazard.Items.ItemVariants
{
    [Item("tool-item"), XmlRoot("tool-item"), XmlType("tool-item")]
    public class BaseToolItem : Item
    {
        [ItemAction("Equip")]
        public void EquipAction(ItemActionParams @params)
        {
            var playObject = @params.CallerObject;
            if (!playObject.TryGetComponent<PlayerBehaviour>(out var player))
                return;
            player.Equip(@params.StoredItem, autoUnequip: true);
        }

        public override IRefItem<Item> Instantiate(int amount)
        {
            RefItem<BaseToolItem> refToolItem = new(ID, amount);
            return refToolItem;
        }

        public virtual BaseTool CreateTool()
        {
            ItemManager itemManager = ItemManager.Manager;

            var toolObject = Object.Instantiate(itemManager.ToolPrefab);
            BaseTool tool = toolObject.AddComponent<BaseTool>();

            return tool;
        }
    }
}