using System;
using CH.Character.Damage.Hediffs;

namespace CH.Character.Damage.HediffDefs
{
    public class InjuryHediffDef : HediffDef<InjuryHediff>
    {
        /// <summary>
        /// The amount of bleeding the injury causes.
        /// </summary>
        public float Bleeding;
        /// <summary>
        /// The amount of pain the injury causes.
        /// </summary>
        public float Pain;

        public override InjuryHediff CreatesAppliedHediff()
        {
            InjuryHediff applied = new();
            return applied;
        }
    }
}
