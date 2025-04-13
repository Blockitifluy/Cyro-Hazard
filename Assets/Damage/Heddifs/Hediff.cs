using CyroHazard.Damage.HediffDefs;
using CyroHazard.Character;

namespace CyroHazard.Damage.Hediffs
{
    /// <summary>
    /// The interface version of hediff. Only used for it's out propetry.                                          
    /// </summary>
    /// <inheritdoc cref="Hediff&lt;TDef&gt;" path="/typeparam[@name='TDef']"/>
    public interface IHediff<out TDef> where TDef : HediffDef
    {
        /// <summary>
        /// This hediff applied to this body part.
        /// </summary>
        public BodyPart AppliedTo { get; }
        /// <summary>
        /// The Character Health script that the Hediff is attached
        /// </summary>
        public CharacterHealth AppliedToHealth { get; }
        /// <summary>
        /// The hediff definition, this hediff is based on.
        /// </summary>
        public TDef HediffDef { get; }
        /// <summary>
        /// The displayed name of the Hediff.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Fires every frame (similar to Unity's Update).
        /// </summary>
        public void OnUpdate();
        /// <summary>
        /// Fires when the hediff is first applied.
        /// </summary>
        public void OnApplied();
    }

    /// <summary>
    /// The base class of all Hediffs (not HediffDefs).
    /// </summary>
    /// <typeparam name="TDef">The HediffDef linked to this Hediff.</typeparam>
    public abstract class Hediff<TDef> : IHediff<TDef> where TDef : HediffDef
    {
        public BodyPart AppliedTo { get; }
        public CharacterHealth AppliedToHealth { get; }
        public TDef HediffDef { get; }

        public virtual string Name => HediffDef.Name;

        public sealed override string ToString() => HediffDef.Name;

        public virtual void OnUpdate() { }

        public virtual void OnApplied() { }

        public Hediff(TDef def, BodyPart bodyPart)
        {
            HediffDef = def;
            AppliedTo = bodyPart;
            AppliedToHealth = bodyPart.CharacterHealth;
        }
    }
}