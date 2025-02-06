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
    }
}