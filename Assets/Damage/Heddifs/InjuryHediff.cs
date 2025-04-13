using CyroHazard.Damage.HediffDefs;
using CyroHazard.Character;
using CyroHazard.Damage.Hediffs.Interface;

namespace CyroHazard.Damage.Hediffs
{
    public class InjuryHediff : Hediff<InjuryHediffDef>, IHediffPain, IHediffBleeding
    {
        public float Severity;
        public bool IsPermanent = false;

        public float Pain => Severity * HediffDef.Pain;
        public float Bleeding => Severity * HediffDef.Bleeding;

        public InjuryHediff(InjuryHediffDef def, BodyPart bodyPart) : base(def, bodyPart) { }
    }
}