using System.Xml.Serialization;
using CyroHazard.Damage.Hediffs;
using CyroHazard.Character;

namespace CyroHazard.Damage.HediffDefs
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

        public override IHediff<HediffDef> CreateAppliedHediff(BodyPart bodyPart)
        {
            InjuryHediff applied = new(this, bodyPart);
            return applied;
        }
    }
}
