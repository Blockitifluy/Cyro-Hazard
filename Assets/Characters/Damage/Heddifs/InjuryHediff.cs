using CH.Character.Damage.HediffDefs;
using UnityEngine;

namespace CH.Character.Damage.Hediffs
{
    public class InjuryHediff : Hediff
    {
        public InjuryHediffDef InjuryHediffDef;
        public float Severity;
        public bool IsPermanent = false;

        public float Pain
        {
            get { return Severity * InjuryHediffDef.Pain; }
        }

        public float Bleeding
        {
            get { return Severity * InjuryHediffDef.Bleeding; }
        }

        public override void OnUpdate()
        {

        }

        public override void OnApplied()
        {
            if (HediffDef is InjuryHediffDef h)
                InjuryHediffDef = h;
        }
    }
}