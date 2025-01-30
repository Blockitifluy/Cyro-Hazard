using CH.Character.Damage.Hediffs;

namespace CH.Character.Damage.HediffDefs
{
    public interface IDef { }

    /// <summary>
    /// The base type of all HediffDefs.
    /// </summary>
    /// <typeparam name="Applied">The applied hediff version.</typeparam>
    public abstract class HediffDef<Applied> : IDef where Applied : Hediff
    {
        /// <summary>
        //1/ Creates the applied hediff.
        /// </summary>
        /// <returns>Applied</returns>
        public abstract Applied CreatesAppliedHediff();
    }
}
