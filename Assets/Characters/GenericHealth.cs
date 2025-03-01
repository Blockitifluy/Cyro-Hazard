using System;
using System.Collections.Generic;
using UnityEngine;

namespace CH.Character
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
        public List<OrganPair> OrganPairs = new(); // TODO - Cache bodyparts

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

        private (BodyPart, BodyPart) GetPairOfBodyPart(OrganPair organPair)
        {
            BodyPart organ1 = BodyParts.Find(bp =>
            {
                return bp.TemplateBP.Name == organPair.Organ1;
            });

            BodyPart organ2 = BodyParts.Find(bp =>
            {
                return bp.TemplateBP.Name == organPair.Organ2;
            });

            return (organ1, organ2);
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
            foreach (OrganPair organPair in OrganPairs)
            {
                var (organ1, organ2) = GetPairOfBodyPart(organPair);

                if (organ1.Health <= 0 && organ2.Health <= 0)
                    return true;
            }

            return false;
        }

        protected override void LoadCapabilities()
        {
            var calculators = new Dictionary<ECapability, CapabilityCal>
            {
                {ECapability.Movement, GetMovementOperation},
                {ECapability.Consciousness, GetConsciousnessOperation},
                {ECapability.Hearing, GetHearingOperation},
                {ECapability.Vision, GetVisionOperation},
                {ECapability.Talking, GetTalkingOperation},
                {ECapability.BloodPumping, GetPumpingOperation},
                {ECapability.BloodProcessing, GetProcessingOperation},
                {ECapability.Manipulation, GetManipulationOperation},
                {ECapability.Eating, GetEatingOperation},
                {ECapability.Digestion, GetDigestionOperation},
            };

            foreach (KeyValuePair<ECapability, CapabilityCal> pair in calculators)
                BodyCalculations.Add(pair.Key, pair.Value);
        }

        // Capability Calculators

        private float GetMovementOperation()
        {
            float operation = GetOrganOperation(ECapability.Movement),
            vision = GetVisionOperation(),
            consciousness = GetConsciousnessOperation();

            return (3 * operation + vision) / 4 * consciousness;
        }

        private float GetConsciousnessOperation()
        {
            float pumping = GetPumpingOperation(),
            processing = GetProcessingOperation();

            float recPain = Pain == 0 ? 1 : (1 - (1 / Pain));

            return pumping * processing / Mathf.Max(recPain / 2, 1.0f);
        }

        private float GetHearingOperation()
        {
            float operation = GetOrganOperation(ECapability.Hearing),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        private float GetVisionOperation()
        {
            float operation = GetOrganOperation(ECapability.Vision),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        private float GetTalkingOperation()
        {
            float operation = GetOrganOperation(ECapability.Talking),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        private float GetPumpingOperation()
        {
            float operation = GetOrganOperation(ECapability.BloodPumping),
            breathing = GetOrganOperation(ECapability.Breathing);

            return breathing * operation;
        }

        private float GetProcessingOperation()
        {
            float operation = GetOrganOperation(ECapability.BloodProcessing),
            pumping = GetPumpingOperation();

            return (operation + pumping) / 2;
        }

        private float GetManipulationOperation()
        {
            float operation = GetOrganOperation(ECapability.Manipulation),
            consciousness = GetConsciousnessOperation();

            return operation * consciousness;
        }

        private float GetEatingOperation()
        {
            float operation = GetOrganOperation(ECapability.Eating),
            consciousness = GetConsciousnessOperation();

            return (3 * operation + consciousness) / 4;
        }

        private float GetDigestionOperation()
        {
            float operation = GetOrganOperation(ECapability.Digestion),
            eating = GetEatingOperation();

            return eating * operation;
        }
    }
}