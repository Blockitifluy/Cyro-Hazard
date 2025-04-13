using System.Xml.Serialization;

namespace CyroHazard.Damage.HediffDefs
{
    interface ISeverityHediffDef
    {
        [XmlElement("max-severity")]
        public float MaxSeverity { get; set; }
        [XmlElement("severity-gain")]
        public float SeverityGain { get; set; }
    }
}