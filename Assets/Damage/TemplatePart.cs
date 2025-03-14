using System;
using System.Collections.Generic;
using UnityEngine;
using CyroHazard.Character;

namespace CyroHazard.Damage
{
    /// <summary>
    /// The template body part used by <see cref="HealthHierarchy"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "TemplatePart", menuName = "Damage System/Template Part")]
    public class TemplatePart : ScriptableObject
    {
        public override bool Equals(object obj)
        {
            if (obj is not TemplatePart template)
                throw new InvalidOperationException();
            return template.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ParentName, IsInside, CanBleed, MaxHealth);
        }

        public static bool operator ==(TemplatePart first, TemplatePart second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(TemplatePart first, TemplatePart second)
        {
            return !(first == second);
        }

        public List<CapabilityReduction> CapabilityReductions;

        /// <summary>
        /// The parent of the template body part.
        /// </summary>
        /// <remarks>
        /// Set as the <see cref="HealthHierarchy.RootBodyPartLabel"/>, if it doesn't have a parent.
        /// </remarks>
        public string ParentName;

        /// <summary>
        /// The name of the body parts.
        /// </summary>
        [Header("Identity")]
        public string Name;

        /// <summary>
        /// Is the body part inside of it's parent.
        /// </summary>
        [Header("Damage")]
        public bool IsInside;
        /// <summary>
        /// Can it bleed?
        /// </summary>
        public bool CanBleed;
        /// <summary>
        /// The max health that the body part could have.
        /// </summary>
        public float MaxHealth;

        public CapabilityReduction? GetCapabilityReduction(ECapability capability)
        {
            foreach (CapabilityReduction capRed in CapabilityReductions)
            {
                if (capability != capRed.Affected) continue;
                return capRed;
            }

            return null;
        }

        public bool IsRootPart() => ParentName != HealthHierarchy.RootBodyPartLabel;
    }
}