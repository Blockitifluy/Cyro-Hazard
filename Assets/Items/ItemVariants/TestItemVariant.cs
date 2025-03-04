using System.Xml.Serialization;

namespace CH.Items.ItemVariants
{
    [Item("variant-item"), XmlRoot("variant-item"), XmlType("variant-item")]
    public class TestItemVariant : BasicItem
    {
        [XmlElement("test-value")]
        public string TestValue;
    }

    public class RefTestItem : RefItem<TestItemVariant>
    {
        public string TestValue;

        public RefTestItem(string id, int amount, string testValue) : base(id, amount)
        {
            TestValue = testValue;
        }
    }
}