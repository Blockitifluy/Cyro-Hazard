using System;
using UnityEngine;
using System.Xml.Serialization;

namespace CH.Items.ItemVariants
{
    [Item("item"), XmlRoot("item"), XmlType("item")]
    /// <summary>
	/// Non-contrete class of item data
	/// </summary>
    public class BasicItem : BaseItem
    {
        public override string ToString()
        {
            return $"{Name} ({ID})";
        }

        public BasicItem() { }
    }
}