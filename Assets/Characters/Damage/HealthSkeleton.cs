using System;
using System.Collections.Generic;
using UnityEngine;

// TODO - Add editor interagetion

namespace CH.Character.Damage
{
    /// <summary>
    /// A skeleton hierachry used for health.
    /// </summary>
    [CreateAssetMenu(fileName = "TemplSkeleton", menuName = "Damage System/HealthSkeleton")]
    public class HealthSkeleton : ScriptableObject
    {
        /// <summary>
        /// If a <see cref="TemplatePart.ParentName"/> is set to this, then it is a root character.
        /// </summary>
        public const string RootBodyPartLabel = "_root_";

        /// <summary>
        /// The name of the <see cref="HealthSkeleton"/>.
        /// </summary>
        public string Name = "Humanoid";
        /// <summary>
        /// A list of body parts (see <see cref="TemplatePart"/>s).
        /// </summary>
        public List<TemplatePart> BodyParts;

        /// <summary>
        /// Thrown if a root part couldn't be found.
        /// </summary>
        [Serializable]
        public class RootPartNotFoundException : Exception
        {
            public RootPartNotFoundException() { }
            protected RootPartNotFoundException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public TemplatePart[] GetChildBodyPart(TemplatePart parentPart)
        {
            List<TemplatePart> bodyParts = new();

            foreach (TemplatePart other in BodyParts)
            {
                if (other.ParentName != parentPart.Name)
                    continue;
                bodyParts.Add(other);
            }

            return bodyParts.ToArray();
        }

        public TemplatePart? GetParent(TemplatePart childPart)
        {
            foreach (TemplatePart other in BodyParts)
            {
                if (childPart.ParentName != childPart.Name)
                    continue;
                return other;
            }

            return null;
        }

        public TemplatePart[] GetAllBodyPartAncestors(TemplatePart childPart)
        {
            if (GetRootPart() == childPart)
                return Array.Empty<TemplatePart>();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the root part of the skeleton.
        /// </summary>
        /// <returns>The root of the Skeleton.</returns>
        /// <exception cref="RootPartNotFoundException">
        /// Thrown if a root part couldn't be found.
        /// </exception>
        public TemplatePart GetRootPart()
        {
            foreach (TemplatePart bodyPart in BodyParts)
            {
                if (bodyPart.ParentName != RootBodyPartLabel)
                    continue;
                return bodyPart;
            }

            throw new RootPartNotFoundException();
        }
    }

    /// <summary>
    /// The template body part used by <see cref="HealthSkeleton"/>.
    /// </summary>
    [Serializable]
    public struct TemplatePart
    {
        public readonly override bool Equals(object obj)
        {
            if (obj is not TemplatePart template)
                throw new InvalidOperationException();
            return template.GetHashCode() == GetHashCode();
        }

        public readonly override int GetHashCode()
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

        /// <summary>
        /// The parent of the template body part.
        /// </summary>
        /// <remarks>
        /// Set as the <see cref="HealthSkeleton.RootBodyPartLabel"/>, if it doesn't have a parent.
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

    }
}
