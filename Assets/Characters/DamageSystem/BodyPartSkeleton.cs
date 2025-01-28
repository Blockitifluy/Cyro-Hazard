using System;
using System.Collections.Generic;
using UnityEngine;

// TODO - Add editor interagetion

namespace CH.Character.DamageSystem
{
    /// <summary>
    /// A skeleton hierachry used for health.
    /// </summary>
    [CreateAssetMenu(fileName = "TemplSkeleton", menuName = "Damage System/HealthSkeleton")]
    public class HealthSkeleton : ScriptableObject
    {
        /// <summary>
        /// If a <see cref="TemplatePart.Parent"/> is set to this, then it is a root character.
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
                if (bodyPart.Parent != RootBodyPartLabel)
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
    public class TemplatePart
    {
        /// <summary>
        /// The max health that the body part could have.
        /// </summary>
        public float MaxHealth = 100.0f;
        /// <summary>
        /// The parent of the template body part.
        /// </summary>
        /// <remarks>
        /// Set as the <see cref="HealthSkeleton.RootBodyPartLabel"/>, if it doesn't have a parent.
        /// </remarks>
        public string Parent = "";
        /// <summary>
        /// The name of the body parts.
        /// </summary>
        [Header("Identity")]
        public string Name = "body part";

        /// <summary>
        /// Is the body part inside of it's parent.
        /// </summary>
        [Header("Damage")]
        public bool IsInside = false;
        /// <summary>
        /// Can it bleed?
        /// </summary>
        public bool CanBleed = true;
    }
}
