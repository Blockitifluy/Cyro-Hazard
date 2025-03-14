using System;
using System.Xml.Serialization;
using CyroHazard.Damage.Hediffs;
using CyroHazard.Character;

namespace CyroHazard.Damage.HediffDefs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed class HediffDefAttribute : Attribute
    {
        public readonly string XMLName;

        public HediffDefAttribute(string xmlName)
        {
            XMLName = xmlName;
        }
    }

    /// <summary>
    /// The base type of all HediffDefs.
    /// </summary>
    public abstract class HediffDef
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = "Unnamed Def";

        public override string ToString()
        {
            return $"{Name} (Def)";
        }

        public abstract IHediff CreateAppliedHediff(BodyPart bodyPart);

        public HediffDef() { }
    }
}
