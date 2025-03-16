using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyroHazard.Character
{
    /// <summary>
    /// Extended from <see cref="CharacterHealth"/>.
    /// </summary>
    public class GenericHealth : CharacterHealth
    {
        /// <summary>
        /// The pair of organ names.
        /// </summary>
        [Serializable]
        public struct OrganPair
        {
            public string Organ1;
            public string Organ2;
        }

        /// <summary>
        /// Pairs that will kill the character, when both destroyed.
        /// </summary>
        [Header("Death")]
        public List<OrganPair> OrganPairs = new();

        private readonly List<(BodyPart, BodyPart)> OrganPairsReal = new();

        /// <summary>
        /// Capabilities that will kill the character, when set to 0%.
        /// </summary>
        public List<ECapability> DeathCapabilities = new();

        protected override bool IsExtendedDead(out EDeathReason deathReason)
        {
            if (DeathByOrganPair())
            {
                deathReason = EDeathReason.OrganGone;
                return true;
            }

            if (DeathByCapability())
            {
                deathReason = EDeathReason.Capability;
                return true;
            }

            deathReason = EDeathReason.None;
            return false;
        }

        public void LoadOrganPairs()
        {
            foreach (OrganPair organPair in OrganPairs)
            {
                BodyPart organ1 = BodyParts.Find(bp =>
                {
                    return bp.TemplateBP.Name == organPair.Organ1;
                });

                BodyPart organ2 = BodyParts.Find(bp =>
                {
                    return bp.TemplateBP.Name == organPair.Organ2;
                });

                OrganPairsReal.Add((organ1, organ2));
            }
        }

        private bool DeathByCapability()
        {
            foreach (ECapability capability in DeathCapabilities)
            {
                if (GetOrganOperation(capability) <= 0)
                    return true;
            }

            return false;
        }

        private bool DeathByOrganPair()
        {
            foreach (var (organ1, organ2) in OrganPairsReal)
            {
                if (organ1.Health > 0 || organ2.Health > 0)
                    continue;
                return true;
            }

            return false;
        }

        // Capability Calculators

        [CapabilityCal(ECapability.Movement)]
        public float GetMovementOperation()
        {
            float operation = GetOrganOperation(ECapability.Movement),
            vision = GetVisionOperation(),
            consciousness = GetConsciousnessOperation();

            return (3 * operation + vision) / 4 * consciousness;
        }

        [CapabilityCal(ECapability.Consciousness)]
        public float GetConsciousnessOperation()
        {
            float pumping = GetPumpingOperation(),
            processing = GetProcessingOperation(),
            pain = Pain,
            painOffset = Mathf.Max(pain * pain / 500, 1);

            return pumping * processing / painOffset;
        }

        [CapabilityCal(ECapability.Hearing)]
        public float GetHearingOperation()
        {
            float operation = GetOrganOperation(ECapability.Hearing),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        [CapabilityCal(ECapability.Vision)]
        public float GetVisionOperation()
        {
            float operation = GetOrganOperation(ECapability.Vision),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        [CapabilityCal(ECapability.Talking)]
        public float GetTalkingOperation()
        {
            float operation = GetOrganOperation(ECapability.Talking),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        [CapabilityCal(ECapability.BloodPumping)]
        public float GetPumpingOperation()
        {
            float operation = GetOrganOperation(ECapability.BloodPumping),
            breathing = GetOrganOperation(ECapability.Breathing);

            return breathing * operation;
        }

        [CapabilityCal(ECapability.BloodProcessing)]
        public float GetProcessingOperation()
        {
            float operation = GetOrganOperation(ECapability.BloodProcessing),
            pumping = GetPumpingOperation();

            return (operation + pumping) / 2;
        }

        [CapabilityCal(ECapability.Manipulation)]
        public float GetManipulationOperation()
        {
            float operation = GetOrganOperation(ECapability.Manipulation),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        [CapabilityCal(ECapability.Eating)]
        public float GetEatingOperation()
        {
            float operation = GetOrganOperation(ECapability.Eating),
            consciousness = GetConsciousnessOperation();

            return (3 * operation + consciousness) / 4;
        }

        [CapabilityCal(ECapability.Digestion)]
        public float GetDigestionOperation()
        {
            float operation = GetOrganOperation(ECapability.Digestion),
            eating = GetEatingOperation();

            return eating * operation;
        }
    }
}