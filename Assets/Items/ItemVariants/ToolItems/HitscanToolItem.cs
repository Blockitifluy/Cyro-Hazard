using System.Xml.Serialization;
using CyroHazard.Tools;
using UnityEngine;

namespace CyroHazard.Items.ItemVariants
{
    // TODO - Add out interface type param
    [Item("hitscan-item"), XmlRoot("hitscan-item"), XmlType("hitscan-item")]
    public class HitscanToolItem : BaseToolItem
    {
        public override IRefItem<Item> Instantiate(int amount)
        {
            RefItem<BaseToolItem> refToolItem = new(ID, amount);
            return refToolItem;
        }

        public override BaseTool CreateTool()
        {
            ItemManager itemManager = ItemManager.Manager;

            var toolObject = Object.Instantiate(itemManager.ToolPrefab);
            HitscanTool tool = toolObject.AddComponent<HitscanTool>();

            return tool;
        }

        [XmlElement("firerate")]
        public float Firerate;
    }
}