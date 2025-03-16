using CyroHazard.Damage.HediffDefs;
using CyroHazard.Character;

namespace CyroHazard.Damage.Hediffs
{
    public interface IHediff<out TDef> where TDef : HediffDef
    {
        /// <summary>
        /// This hediff applied to this body part.
        /// </summary>
        public BodyPart AppliedTo { get; set; }
        /// <summary>
        /// The hediff definition, this hediff is based on.
        /// </summary>
        public TDef HediffDef { get; }

        public void OnUpdate();
        public void OnApplied();
    }

    /// <summary>
    /// The base class of all Hediffs (excluding HediffDefs).
    /// </summary>
    public abstract class Hediff<TDef> : IHediff<TDef> where TDef : HediffDef
    {
        public BodyPart AppliedTo { get; set; }
        public TDef HediffDef { get; }

        public virtual string Name
        {
            get { return HediffDef.Name; }
        }

        public override string ToString()
        {
            return HediffDef.Name;
        }

        public virtual void OnUpdate() { }

        public virtual void OnApplied() { }

        public Hediff(TDef def, BodyPart bodyPart)
        {
            HediffDef = def;
            AppliedTo = bodyPart;
        }
    }
}