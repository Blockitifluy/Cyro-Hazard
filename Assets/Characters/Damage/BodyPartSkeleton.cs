using System;
using System.Collections.Generic;
using UnityEngine;

namespace CH.Character.DamageSystem
{
    [CreateAssetMenu(fileName = "BodyPartSkeleton", menuName = "Damage System/Body Part Skeleton")]
    public class BodyPartSkeleton : ScriptableObject
    {
        public const string RootBodyPartLabel = "_root_";

        [Serializable]
        public class BodyPart
        {
            public float MaxHealth = 100.0f;
            public string Parent = "";

            [Header("Identity")]
            public string ID = "unknown part";
            public string Name = "body part";

            [Header("Damage")]
            public bool IsInside = false;
            public bool CanBleed = true;
        }

        public string Name = "Humanoid";
        public List<BodyPart> BodyParts;

        public BodyPart RootBodyPart
        {
            get
            {
                foreach (BodyPart prt in BodyParts)
                {
                    if (prt.ID != RootBodyPartLabel)
                        continue;
                    return prt;
                }
                return null;
            }
        }
    }
}
