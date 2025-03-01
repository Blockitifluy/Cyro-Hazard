using System.Collections.Generic;
using CH.Character.Damage;
using CH.Character.Damage.Hediffs;
using System;
using UnityEngine;

namespace CH.Character
{
    /// <summary>
    /// The type of capabilities.
    /// </summary>
    public enum ECapability : byte
    {
        Consciousness, // Awareness
        Movement,
        Manipulation, // As in physical

        Vision, // Sight
        Talking,
        Hearing,
        Eating, // Not be confused with Digestion, about eating speed

        BloodPumping, // Heart
        BloodProcessing, // Kidneys and Liver

        Breathing,
        Digestion // Not be confused with Eating, about the stomach
    }

    /// <summary>
    /// Used by the <see cref="BodyPart"/> struct.
    /// Controls how much reduction is taken when destroyed.
    /// </summary>
    [Serializable]
    public struct CapabilityReduction
    {
        /// <summary>
        /// The capability being affected.
        /// </summary>
        public ECapability Affected;

        /// <summary>
        /// The reduction when the body part is being destroyed.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float DestroyedReduction;

        public CapabilityReduction(ECapability affected, float destroyedReduction)
        {
            Affected = affected;
            DestroyedReduction = destroyedReduction;
        }
    }

    /// <summary>
    /// Contains character's health, using a health per body part system.
    /// The character will die when the <c>Health</c> is under <c>DieUnderHealth</c>.
    /// </summary>
    public abstract class CharacterHealth : MonoBehaviour
    {
        // Public Fields & Propetries

        public delegate float CapabilityCal();

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
        public List<TemplatePart> TemplateBody => Hierachry.TemplateParts;

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

        /// <summary>
        /// The amount of pain, the character is experiencing.
        /// </summary>
        public float Pain
        {
            get
            {
                float pain = 0;

                foreach (TemplatePart templatePart in TemplateBody)
                {
                    var bodypart = GetCharBodyPart(templatePart);
                    if (!bodypart.HasValue)
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

        public Dictionary<ECapability, CapabilityCal> BodyCalculations = new();

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
            OrganGone,
            Capability,
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

            bool deathFromAbilities = IsExtendedDead(out var reason);
            if (deathFromAbilities)
            {
                deathReason = reason;
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
            foreach (BodyPart other in BodyParts)
            {
                if (other.TemplateBP != templatePart)
                    continue;
                return other;
            }
            return null;
        }

        /// <summary>
        /// Get the total organ operation of the character's <paramref name="capability"/>.
        /// </summary>
        /// <param name="capability">The capability being measured.</param>
        /// <returns>From 0 to 1 (being the regular amount).</returns>
        /// <exception cref="NullReferenceException">Thrown if a template body is invalid.</exception>
        public float GetOrganOperation(ECapability capability)
        {
            float operation = 1;

            foreach (var templPart in TemplateBody)
            {
                BodyPart? bodyPart = GetCharBodyPart(templPart);

                if (!bodyPart.HasValue)
                    throw new NullReferenceException("Bodypart was null!");

                try
                {
                    float bpOperation = bodyPart.Value.GetOrganOperation(capability);
                    operation -= bpOperation;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }

            return MathF.Max(operation, 0.0f);
        }

        public float GetCapability(ECapability capability)
        {
            var hasCal = BodyCalculations.ContainsKey(capability);

            if (!hasCal) return GetOrganOperation(capability);

            CapabilityCal cal = BodyCalculations[capability];

            return cal();
        }

        // Tests

#if UNITY_EDITOR
        /// <summary>
        /// Injures a random body part by 3 health.
        /// </summary>
        [ContextMenu("Injure Random Part")]
        public void InjureRandomPart()
        {
            int index = UnityEngine.Random
                .Range(0, TemplateBody.Count);

            DamageSystem damageSystem = DamageSystem.GetDamageSystem();

            damageSystem.InjureCharacterBP("Cut", BodyParts[index], 3.0f);
        }

        /// <summary>
        /// Destroys a random body part.
        /// </summary>
        /// <remarks>
        /// Doesn't actually destroy (does 1e5 damage).
        /// </remarks>
        [ContextMenu("Destroy Random Part")]
        public void DestroyRandomPart()
        {
            int index = UnityEngine.Random
                .Range(0, TemplateBody.Count);

            DamageSystem damageSystem = DamageSystem.GetDamageSystem();

            damageSystem.InjureCharacterBP("Cut", BodyParts[index], 100000.0f);
        }

        /// <summary>
        /// Prints if the character is dead, and if by what reason.
        /// </summary>
        [ContextMenu("Is Dead?")]
        public void TestCheckIfDead()
        {
            bool isDead = IsDead(out var reason);

            if (!isDead)
            {
                Debug.Log("It's alive");
                return;
            }

            Debug.Log($"The Character is Dead (reason: {reason})!");
        }

        /// <summary>
        /// Prints all organ operation.
        /// </summary>
        [ContextMenu("Print All Organ Operation")]
        public void PrintAllOrganOperation()
        {
            var capabilities = (ECapability[])Enum.GetValues(typeof(ECapability));
            List<string> operations = new();

            foreach (ECapability capability in capabilities)
            {
                operations.Add($"{capability}: {GetOrganOperation(capability)}");
            }

            Debug.Log(string.Join(", ", operations));
        }

        /// <summary>
        /// Prints all organ operation.
        /// </summary>
        [ContextMenu("Print All Capability")]
        public void PrintAllCapability()
        {
            var capabilities = (ECapability[])Enum.GetValues(typeof(ECapability));
            List<string> operations = new();

            foreach (ECapability capability in capabilities)
            {
                operations.Add($"{capability}: {GetCapability(capability)}");
            }

            Debug.Log(string.Join(", ", operations));
        }
#endif
        // Protected Methods

        protected abstract bool IsExtendedDead(out EDeathReason deathReason);

        // Private Methods

        /// <summary>
        /// Calls the <c>Update</c> method to applied body parts. 
        /// </summary>
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

        protected abstract void LoadCapabilities();

        // Unity

        public void Update()
        {
            UpdateAllHediffs();
        }

        public void Awake()
        {
            LoadAllBodyParts();
            LoadCapabilities();
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

                return Mathf.Clamp(health, 0, TemplateBP.MaxHealth);
            }
        }

        /// <summary>
        /// The amount of pain this body part is causing.
        /// </summary>
        public readonly float Pain
        {
            get
            {
                float pain = 0;

                foreach (Hediff hediff in AppliedHedfiffs)
                {
                    if (hediff is not InjuryHediff injury)
                        continue;
                    pain += injury.InjuryHediffDef.Pain * injury.Severity;
                }

                return pain;
            }
        }

        /// <summary>
        /// Gets organ operation of this body part.
        /// </summary>
        /// <param name="capability">The capability being measured.</param>
        /// <returns>A value usually between 0 to 1.</returns>
        /// <exception cref="NullReferenceException">Thrown if the capability won't be reduced.</exception>
        public readonly float GetOrganOperation(ECapability capability)
        {
            float percent = Health / TemplateBP.MaxHealth;

            var capabilityReduct = TemplateBP.GetCapabilityReduction(capability);
            if (!capabilityReduct.HasValue)
                throw new NullReferenceException("Capability Reduction was Null!");

            return capabilityReduct.Value.DestroyedReduction * (1 - percent);
        }

        /// <summary>
        /// The hediffs being applied on the body part.
        /// </summary>
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