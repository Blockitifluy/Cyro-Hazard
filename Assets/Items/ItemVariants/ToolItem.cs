using System.Xml.Serialization;
using CH.Character.Player;
using CH.Tools;
using UnityEngine;

namespace CH.Items.ItemVariants
{
    [Item("tool-item"), XmlRoot("tool-item"), XmlType("tool-item")]
    public class ToolItem : BaseItem
    {
        [ItemAction("Equip")]
        public void EquipAction(ItemActionParams @params)
        {
            var playObject = @params.CallerObject;
            if (!playObject.TryGetComponent<PlayerBehaviour>(out var player))
                return;
            player.Equip(@params.StoredItem, autoUnequip: true);
        }

        public override IRefItem<BaseItem> Instantiate(int amount)
        {
            RefItem<BaseItem> refToolItem = new(ID, amount);
            return refToolItem;
        }

        public static BaseTool CreateTool<TItem>() where TItem : ToolItem
        {
            ItemManager itemManager = ItemManager.Manager;

            var toolObject = Object.Instantiate(itemManager.ToolPrefab);
            BaseTool tool = toolObject.AddComponent<BaseTool>();

            return tool;
        }

        [XmlElement("firerate")]
        public float Firerate;
    }
}