using CH.Character.Damage.Hediffs;

namespace CH.Character.Damage.HediffDefs
{
    public class SeverityHediffDef : HediffDef<SeverityHediff>
    {
        public float MaxSeverity = 1;
        public float SeverityGain = 0.005f;

        public override SeverityHediff CreatesAppliedHediff()
        {
            SeverityHediff severityHediff = new();
            return severityHediff;
        }
    }
}