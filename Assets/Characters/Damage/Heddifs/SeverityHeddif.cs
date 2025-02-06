
using CH.Character.Damage.HediffDefs;

namespace CH.Character.Damage.Hediffs
{
    public class SeverityHediff : Hediff
    {
        public SeverityHediffDef SeverityHediffDef;
        public float Severity = 0.0f;

        private bool _Debounce = false;

        public override void OnApplied()
        {
            if (HediffDef is SeverityHediffDef severityHediffDef)
                SeverityHediffDef = severityHediffDef;
        }

        public virtual void OnMaxSeverity() { }

        public override void OnUpdate()
        {
            if (_Debounce)
                return;

            if (Severity < SeverityHediffDef.MaxSeverity)
                Severity += SeverityHediffDef.SeverityGain;
            else
            {
                OnMaxSeverity();
                _Debounce = true;
            }
        }
    }
}
