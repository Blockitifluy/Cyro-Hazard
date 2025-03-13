using System.Xml.Serialization;
using CyroHazard.Character.Damage.Hediffs;

namespace CyroHazard.Character.Damage.HediffDefs
{
    [HediffDef("severity-hediff"), XmlRoot("severity-hediff"), XmlType("severity-hediff")]
    public class SeverityHediffDef : HediffDef
    {
        [XmlElement("max-severity")]
        public float MaxSeverity = 1;
        [XmlElement("severity-gain")]
        public float SeverityGain = 0.005f;

        public override IHediff CreateAppliedHediff(BodyPart bodyPart)
        {
            SeverityHediff applied = new(this, bodyPart);
            return (IHediff)applied;
        }
    }
}