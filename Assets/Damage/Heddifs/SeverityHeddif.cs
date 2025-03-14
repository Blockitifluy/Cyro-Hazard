using CyroHazard.Damage.HediffDefs;
using CyroHazard.Damage.Hediffs;

namespace CyroHazard.Character.Damage.Hediffs
{
    public class SeverityHediff : Hediff<SeverityHediffDef>
    {
        public float Severity = 0.0f;

        private bool _Debounce = false;



        public virtual void OnMaxSeverity() { }

        public override void OnUpdate()
        {
            if (_Debounce)
                return;

            if (Severity < HediffDef.MaxSeverity)
                Severity += HediffDef.SeverityGain;
            else
            {
                OnMaxSeverity();
                _Debounce = true;
            }
        }

        public SeverityHediff(SeverityHediffDef def, BodyPart bodyPart) : base(def, bodyPart) { }
    }
}
