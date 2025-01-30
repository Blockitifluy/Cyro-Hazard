using CH.Character.Damage.HediffDefs;

namespace CH.Character.Damage.Hediffs
{
    /// <summary>
    /// The base class of all Hediffs (excluding HediffDefs).
    /// </summary>
    public abstract class Hediff
    {
        /// <summary>
        /// This hediff applied to this body part.
        /// </summary>
        public CharacterBP AppliedTo;
        public IDef HediffDef;

        public abstract void OnUpdate();

        public abstract void OnApplied();

    }
}