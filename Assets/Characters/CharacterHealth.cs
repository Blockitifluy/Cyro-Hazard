using System.Collections.Generic;
using CH.Character.DamageSystem;
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
        public List<TemplatePart> BodyParts
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

                foreach (var prt in BodyParts)
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

                for (int i = 0; i < BodyParts.Count; i++)
                {
                    CharacterPart appliedPart = _CharBodyParts[i];
                    health += appliedPart.Health;
                }

                return health;
            }
        }

        // Private Fields & Propetries

        /// <summary>
        /// The character's body parts, containing the <see cref="CharacterPart.Health"/> propetry.
        /// </summary>
        private readonly List<CharacterPart> _CharBodyParts = new();

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
        /// <param name="bodyPart">The <see cref="TemplatePart"/> that the character has.</param>
        /// <returns>The Character's Body Part</returns>
        public CharacterPart GetCharBodyPart(TemplatePart bodyPart)
        {
            int index = BodyParts.BinarySearch(bodyPart);
            return _CharBodyParts[index];
        }

        /// <summary>
        /// Gets the body part by index.
        /// </summary>
        /// <param name="index">The index of the body part.</param>
        /// <returns>The Character's Body Part</returns>
        public CharacterPart GetCharBodyPartByIndex(int index) => _CharBodyParts[index];

        // Private Methods

        private bool DeathByOrganLoss()
        {
            for (int i = 0; i < BodyParts.Count; i++)
            {
                CharacterPart charBody = GetCharBodyPartByIndex(i);
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
                CharacterPart bodyPart = new(templPart);
                _CharBodyParts.Add(bodyPart);
            }
        }

        // Unity

        public void Awake()
        {
            LoadAllBodyParts();
        }
    }

    /// <summary>
    /// A body part apart from <c><see cref="CharacterHealth"/></c>.
    /// </summary>
    public class CharacterPart
    {
        /// <summary>
        /// The body part that the object derives from.
        /// </summary>
        public readonly TemplatePart BodyPart;
        /// <summary>
        /// The health of the body part.
        /// </summary>
        /// <remarks>
        /// In range of 0 to <see cref="TemplatePart.MaxHealth"/>.
        /// </remarks>
        public float Health
        {
            get { return _Health; }
            set
            {
                _Health = Mathf.Clamp(value, 0, BodyPart.MaxHealth);
            }
        }

        /// <inheritdoc cref="Health"/>
        private float _Health;

        public override string ToString()
        {
            return BodyPart.Name;
        }

        public CharacterPart(TemplatePart bodyPart)
        {
            BodyPart = bodyPart;
            _Health = bodyPart.MaxHealth;
        }
    }
}