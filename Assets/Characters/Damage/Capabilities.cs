using System;
using System.Collections.Generic;
using UnityEngine;

namespace CH.Character.Damage
{
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

    [Serializable]
    public struct CapabilityReduction
    {
        public ECapability Affected;
        [Range(0.0f, 1.0f)]
        public float DestroyedReduction;

        public CapabilityReduction(ECapability affected, float destroyedReduction)
        {
            Affected = affected;
            DestroyedReduction = destroyedReduction;
        }
    }

    public abstract class Capabilities : MonoBehaviour
    {
        // TODO
        public CharacterHealth CharacterHealth;

        public float GetOrganOperation(ECapability capability)
        {
            float operation = 1;

            foreach (var templPart in CharacterHealth.TemplateBody)
            {
                BodyPart? bodyPart = CharacterHealth.GetCharBodyPart(templPart);

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

            return operation;
        }

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
    }
}
