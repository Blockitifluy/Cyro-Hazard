using System.Xml.Serialization;
using CyroHazard.Character.Damage.Hediffs;

namespace CyroHazard.Character.Damage.HediffDefs
{
    [HediffDef("injury-hediff"), XmlRoot("injury-hediff"), XmlType("injury-hediff")]
    public class InjuryHediffDef : HediffDef
    {
        /// <summary>
        /// The amount of bleeding the injury causes.
        /// </summary>
        [XmlElement("bleeding")]
        public float Bleeding;
        /// <summary>
        /// The amount of pain the injury causes.
        /// </summary>
        [XmlElement("pain")]
        public float Pain;

        public override IHediff CreateAppliedHediff(BodyPart bodyPart)
        {
            InjuryHediff applied = new(this, bodyPart);
            return (IHediff)applied;
        }
    }
}
