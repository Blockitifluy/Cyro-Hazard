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
        public HealthSkeleton Skeleton;

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
            get { return Skeleton.BodyParts; }
        }

        /// <summary>
        /// The total max health of the character.
        /// </summary>
        public float MaxHealth
        {
            get
            {
                float maxHealth = 0;

                foreach (var prt in TemplateBody)
                {
                    maxHealth += prt.MaxHealth;
                }

                return maxHealth;
            }
        }

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
                    BodyPart appliedPart = _CharBodyParts[i];
                    health += appliedPart.Health;
                }

                return health;
            }
        }

        // Private Fields & Propetries

        /// <summary>
        /// The character's body parts, containing the <see cref="BodyPart.Health"/> propetry.
        /// </summary>
        private readonly List<BodyPart> _CharBodyParts = new();

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
            float health = Health;

            if (health * DieUnderHealthPer >= health)
            {
                deathReason = EDeathReason.Health;
                return true;
            }

            if (DeathByOrganLoss())
            {
                deathReason = EDeathReason.OrganGone;
                return true;
            }

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
            foreach (BodyPart other in _CharBodyParts)
            {
                if (other.TemplateBP != templatePart)
                    continue;
                return other;
            }
            return null;
        }

        /// <summary>
        /// Gets the body part by index.
        /// </summary>
        /// <param name="index">The index of the body part.</param>
        /// <returns>The Character's Body Part</returns>
        public BodyPart? GetCharBodyPartByIndex(int index) => _CharBodyParts[index];

        [ContextMenu("Injure Random Part")]
        public void InjureRandomPart()
        {
            int index = UnityEngine.Random
                .Range(0, TemplateBody.Count);

            DamageSystem damageSystem = DamageSystem.GetDamageSystem();

            damageSystem.InjureCharacterBP("Cut", _CharBodyParts[index], 3);
        }

        // Private Methods

        private void UpdateAllHediffs()
        {
            foreach (BodyPart bodyPart in _CharBodyParts)
            {
                foreach (Hediff hediff in bodyPart.AppliedHedfiffs)
                {
                    hediff.OnUpdate();
                }
            }
        }

        private bool DeathByOrganLoss()
        {
            for (int i = 0; i < TemplateBody.Count; i++)
            {
                BodyPart charBody = GetCharBodyPartByIndex(i).Value;
                if (charBody.Health <= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Loads all the body parts from the <see cref="HealthSkeleton.BodyParts"/> field.
        /// </summary>
        private void LoadAllBodyParts()
        {
            _CharBodyParts.Clear();

            foreach (TemplatePart templPart in Skeleton.BodyParts)
            {
                BodyPart bodyPart = new(templPart);
                _CharBodyParts.Add(bodyPart);
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
        public float Health
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

        public List<Hediff> AppliedHedfiffs;

        public override string ToString()
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