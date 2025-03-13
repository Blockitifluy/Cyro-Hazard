using CyroHazard.Character.Damage.HediffDefs;

namespace CyroHazard.Character.Damage.Hediffs
{
    public class InjuryHediff : Hediff<InjuryHediffDef>
    {
        public float Severity;
        public bool IsPermanent = false;

        public float Pain => Severity * HediffDef.Pain;
        public float Bleeding => Severity * HediffDef.Bleeding;

        public InjuryHediff(InjuryHediffDef def, BodyPart bodyPart) : base(def, bodyPart) { }
    }
}