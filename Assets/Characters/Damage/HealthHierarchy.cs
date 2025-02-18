using System;
using System.Collections.Generic;
using UnityEngine;

namespace CH.Character.Damage
{
    /// <summary>
    /// A skeleton hierachry used for health.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthHierarchy", menuName = "Damage System/HealthHierarchy")]
    public class HealthHierarchy : ScriptableObject
    {
        /// <summary>
        /// If a <see cref="TemplatePart.ParentName"/> is set to this, then it is a root character.
        /// </summary>
        public const string RootBodyPartLabel = "_root_";

        /// <summary>
        /// The name of the <see cref="HealthHierarchy"/>.
        /// </summary>
        public string Name = "Humanoid";
        /// <summary>
        /// A list of body parts (see <see cref="TemplatePart"/>s).
        /// </summary>
        public List<TemplatePart> TemplateParts;

        public float MaxHealth
        {
            get
            {
                float maxHealth = 0;

                foreach (var prt in TemplateParts)
                {
                    maxHealth += prt.MaxHealth;
                }

                return maxHealth;
            }
        }

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

            foreach (TemplatePart other in TemplateParts)
            {
                if (other.ParentName != parentPart.Name)
                    continue;
                bodyParts.Add(other);
            }

            return bodyParts.ToArray();
        }

        public TemplatePart GetParent(TemplatePart childPart)
        {
            foreach (TemplatePart other in TemplateParts)
            {
                if (childPart.ParentName != childPart.Name)
                    continue;
                return other;
            }

            return null;
        }

        public TemplatePart[] GetAllBodyPartAncestors(TemplatePart childPart)
        {
            if (childPart.IsRootPart())
                return Array.Empty<TemplatePart>();

            List<TemplatePart> result = new();
            TemplatePart current = GetParent(childPart);

            while (!current.IsRootPart())
            {
                TemplatePart now = GetParent(current);
                result.Add(now);
                current = now;
            }

            return result.ToArray();
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
            foreach (TemplatePart bodyPart in TemplateParts)
            {
                if (!bodyPart.IsRootPart())
                    continue;
                return bodyPart;
            }

            throw new RootPartNotFoundException();
        }
    }
}
