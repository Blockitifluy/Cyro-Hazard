using System.Collections.Generic;
using CH.Character.Damage;
using CH.Character.Damage.Hediffs;
using System;
using UnityEngine;

namespace CH.Character
{
    /// <summary>
    /// Contains character's health, using a health per body part system.
    /// The character will die when the <c>Health</c> is under <c>DieUnderHealth</c>.
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        // Public Fields & Propetries

        /// <summary>
        /// The health skeleton of the character.
        /// </summary>
        public HealthHierarchy Hierachry;

        /// <summary>
        /// When the character's health is under this percent, then the character is considered dead.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float DieUnderHealthPer = .2f;

        /// <summary>
        /// The template body parts of
        /// </summary>
        public List<TemplatePart> TemplateBody
        {
            get { return Hierachry.TemplateParts; }
        }

        /// <summary>
        /// The total max health of the character.
        /// </summary>
        public float MaxHealth => Hierachry.MaxHealth;

        /// <summary>
        /// The total health of the character.
        /// </summary>
        public float Health
        {
            get
            {
                float health = 0;

                for (int i = 0; i < TemplateBody.Count; i++)
                {
                    BodyPart appliedPart = BodyParts[i];
                    health += appliedPart.Health;
                }

                return health;
            }
        }

        public float Pain
        {
            get
            {
                float pain = 0;

                foreach (TemplatePart templatePart in TemplateBody)
                {
                    var bodypart = GetCharBodyPart(templatePart);
                    if (bodypart.HasValue)
                        continue;
                    pain += bodypart.Value.Pain;
                }

                return pain;
            }
        }

        /// <summary>
        /// The character's body parts, containing the <see cref="BodyPart.Health"/> propetry.
        /// </summary>
        public readonly List<BodyPart> BodyParts = new();

        // Public Methods

        /// <summary>
        /// The reason for the character's death.
        /// </summary>
        /// <remarks>
        /// <c>None</c> means the character didn't die.
        /// </remarks>
        public enum EDeathReason : uint
        {
            None,
            Health,
            OrganGone
        }

        /// <summary>
        /// Is the character dead?
        /// </summary>
        /// <param name="deathReason">Returns the reason why the character died.</param>
        /// <returns>Self-Explainitary</returns>
        public bool IsDead(out EDeathReason deathReason)
        {
            if (MaxHealth * DieUnderHealthPer >= Health)
            {
                deathReason = EDeathReason.Health;
                return true;
            }

            // if (DeathByOrganLoss())
            // {
            //     deathReason = EDeathReason.OrganGone;
            //     return true;
            // }

            deathReason = EDeathReason.None;
            return false;
        }

        /// <summary>
        /// The percentage of health that the character has.
        /// </summary>
        /// <returns>The percentage of health the character has.</returns>
        public float GetHealthPercent()
        {
            return Health / MaxHealth;
        }

        /// <summary>
        /// Gets the body part from the <see cref="TemplatePart"/>.
        /// </summary>
        /// <param name="templatePart">The <see cref="TemplatePart"/> that the character has.</param>
        /// <returns>The Character's Body Part</returns>
        public BodyPart? GetCharBodyPart(TemplatePart templatePart)
        {
            foreach (BodyPart other in BodyParts)
            {
                if (other.TemplateBP != templatePart)
                    continue;
                return other;
            }
            return null;
        }

        [ContextMenu("Injure Random Part")]
        public void InjureRandomPart()
        {
            int index = UnityEngine.Random
                .Range(0, TemplateBody.Count);

            DamageSystem damageSystem = DamageSystem.GetDamageSystem();

            damageSystem.InjureCharacterBP("Cut", BodyParts[index], 3);
        }

        // Private Methods

        private void UpdateAllHediffs()
        {
            foreach (BodyPart bodyPart in BodyParts)
            {
                foreach (Hediff hediff in bodyPart.AppliedHedfiffs)
                {
                    hediff.OnUpdate();
                }
            }
        }

        /// <summary>
        /// Loads all the body parts from the <see cref="HealthHierarchy.TemplateParts"/> field.
        /// </summary>
        private void LoadAllBodyParts()
        {
            BodyParts.Clear();

            foreach (TemplatePart templPart in Hierachry.TemplateParts)
            {
                BodyPart bodyPart = new(templPart);
                BodyParts.Add(bodyPart);
            }
        }

        // Unity

        public void Update()
        {
            UpdateAllHediffs();
        }

        public void Awake()
        {
            LoadAllBodyParts();
        }
    }

    /// <summary>
    /// A body part apart from <c><see cref="CharacterHealth"/></c>.
    /// </summary>
    public struct BodyPart
    {
        /// <summary>
        /// The body part that the object derives from.
        /// </summary>
        public readonly TemplatePart TemplateBP;
        /// <summary>
        /// The health of the body part.
        /// </summary>
        /// <remarks>
        /// In range of 0 to <see cref="TemplatePart.MaxHealth"/>.
        /// </remarks>
        public readonly float Health
        {
            get
            {
                float health = TemplateBP.MaxHealth;

                foreach (Hediff hediff in AppliedHedfiffs)
                {
                    if (hediff is not InjuryHediff injury)
                        continue;
                    health -= injury.Severity;
                }

                return health;
            }
        }

        public readonly float Pain
        {
            get
            {
                float pain = 0;

                foreach (Hediff hediff in AppliedHedfiffs)
                {
                    if (hediff is not InjuryHediff injury)
                        continue;
                    pain += injury.InjuryHediffDef.Pain;
                }

                return pain;
            }
        }

        public readonly float GetOrganOperation(ECapability capability)
        {
            float percent = Health / TemplateBP.MaxHealth;

            var capabilityReduct = TemplateBP.GetCapabilityReduction(capability);
            if (!capabilityReduct.HasValue)
                throw new NullReferenceException("Capability Reduction was Null!");

            return capabilityReduct.Value.DestroyedReduction * (1 - percent);
        }

        public List<Hediff> AppliedHedfiffs;

        public override readonly string ToString()
        {
            return $"{TemplateBP.Name} ({Health})";
        }

        public BodyPart(TemplatePart bodyPart)
        {
            TemplateBP = bodyPart;
            AppliedHedfiffs = new();
        }
    }
}