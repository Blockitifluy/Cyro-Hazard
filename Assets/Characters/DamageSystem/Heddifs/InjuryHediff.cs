using CH.Character.Damage.HediffDefs;
using UnityEngine;

namespace CH.Character.Damage.Hediffs
{
    public class InjuryHediff : Hediff
    {
        public InjuryHediffDef InjuryHediffDef;
        public float Severity;
        public bool IsPermanent = false;

        public override void OnUpdate()
        {
            Debug.Log("OUCH!");
        }

        public override void OnApplied()
        {
            if (HediffDef is InjuryHediffDef h)
                InjuryHediffDef = h;
        }
    }
}