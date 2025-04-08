using System.Xml.Serialization;

namespace CyroHazard.Items.ItemVariants
{
    [Item("item"), XmlRoot("item"), XmlType("item")]
    /// <summary>
	/// Non-contrete class of item data
	/// </summary>
    public class BasicItem : Item
    {
        public override string ToString()
        {
            return $"{Name} ({ID})";
        }

        public BasicItem() { }
    }
}