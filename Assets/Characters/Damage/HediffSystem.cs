using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;
using CH.Character.Damage.Hediffs;
using CH.Character.Damage.HediffDefs;
using System.Xml.Serialization;
using System.IO;

namespace CH.Character.Damage
{
    public struct InjuryResult
    {
        public InjuryHediff BaseHediff;

        public InjuryResult(InjuryHediff baseHediff)
        {
            BaseHediff = baseHediff;
        }
    }

    public partial class DamageSystem : MonoBehaviour
    {
        // Constants

        /// <summary>
        /// Path to Hediff Types xml. Used for <see cref="PreloadHediffs"/>.
        /// </summary>
        public const string PathToHediffTypes = "Data\\DamageSystem\\Hediffs\\Hediff.xml";

        // Pre-Loading

        /// <summary>
        /// The XML document used to load in all <see cref="IHediffDef"/>s.
        /// </summary>
        static private XmlDocument HediffDocument;

        /// <summary>
        /// Preloads all hediff types. Preferable on the awake function.
        /// </summary>
        /// <exception cref="InvalidCastException">The element type can not be converted.</exception>
        private void PreloadHediffs()
        {
            var hediffElements = HediffDocument.DocumentElement.ChildNodes;
            int loaded = 0;

            foreach (XmlElement element in hediffElements)
            {
                Type hediffType = default;

                foreach (KeyValuePair<string, Type> pair in HediffTypes)
                {
                    if (pair.Key != element.Name)
                        continue;
                    hediffType = pair.Value;
                }

                XmlSerializer serializer = new(hediffType);

                using StringReader reader = new(element.OuterXml);
                HediffDef hediff = (HediffDef)serializer.Deserialize(reader);

                if (hediff is null)
                    throw new InvalidCastException($"Hediff name {element.Name} is invailid");

                string keyName = element.GetAttribute("name").Replace('-', ' ');

                hediff.Name = keyName;
                Hediffs[keyName] = hediff;
                loaded++;
            }

            Debug.Log($"Loaded all {loaded} hediff(s)");
        }

        private void PreloadHediffTypes()
        {
            var itemTypes = Helper.GetTypesWithAttribute<HediffDefAttribute>(GetType().Assembly);

            foreach (Type tp in itemTypes)
            {
                HediffDefAttribute itemAtt = (HediffDefAttribute)Attribute.GetCustomAttribute(tp, typeof(HediffDefAttribute));
                HediffTypes.Add(itemAtt.XMLName, tp);
            }
        }

        // Retrieving

        /// <inheritdoc cref="GetHediffDef"/>
        public DefT GetHediffDef<DefT>(string name) where DefT : HediffDef
        {
            return GetHediffDef(name) as DefT;
        }

        // <summary>
        /// Gets the HediffDef by name then converts it to <typeparamref name="DefT"/> param type.
        /// </summary>
        /// <typeparam name="DefT">Needs to derive from <see cref="IHediffDef"/>.</typeparam>
        /// <param name="name">The name of the <see cref="HediffDef"/>.</param>
        /// <returns>The HediffDef</returns>
        /// <exception cref="InvalidCastException">Thrown of the Hediff could be converted in to <typeparamref name="DefT"/>.</exception>
        public HediffDef GetHediffDef(string name)
        {
            var hediff = Hediffs[name];

            return hediff;
        }

        /// <summary>
        /// All the hediff types (in name, IDef format)
        /// </summary>
        private readonly Dictionary<string, HediffDef> Hediffs = new();
        private readonly Dictionary<string, Type> HediffTypes = new();

        // Injury

        /// <summary>
        /// Injures a character's <paramref name="bodyPart"/> with a damage type
        /// </summary>
        /// <remarks>
        /// This function doesn't do piercing
        /// </remarks>
        /// <param name="damageName">The name of the damage type.</param>
        /// <param name="bodyPart">The body part affected.</param>
        /// <param name="damage">The severity of the damage.</param>
        /// <param name="isPermanent">Is the injury permanent?</param>
        /// <returns>The injury hediff applied</returns>
        public InjuryHediff InjureCharacterBP(string damageName, BodyPart bodyPart, float damage, bool isPermanent)
        {
            DamageType damageType = GetDamageTypeFromName(damageName);

            var hediff = (InjuryHediff)ApplyHediff(damageType.Applies, bodyPart);
            hediff.Severity = damage;
            hediff.IsPermanent = isPermanent;

            return hediff;
        }

        /// <inheritdoc cref="InjureCharacterBP(string, BodyPart, float, bool)"/>
        public InjuryHediff InjureCharacterBP(string damageName, BodyPart bodyPart, float damage)
        {
            return InjureCharacterBP(damageName, bodyPart, damage, false);
        }

        // Applitation

        /// <inheritdoc cref="ApplyHediff(string, BodyPart)"/>
        /// <param name="def">A hediff defination.</param>
        public IHediff ApplyHediff<TDef>(TDef def, BodyPart bodyPart) where TDef : HediffDef
        {
            var applied = def.CreateAppliedHediff(bodyPart);
            bodyPart.AppliedHedfiffs.Add(applied);
            applied.OnApplied();
            return applied;
        }

        /// <summary>
        /// Applies a hediff to a bodypart.
        /// </summary>
        /// <typeparam name="DefT">The hediff definition.</typeparam>
        /// <typeparam name="HediffT">The applied definition.</typeparam>
        /// <param name="bodyPart">The bodypart, the hediff is going to be applied on.</param>
        /// <param name="defName">The name of hediff def</param>
        /// <returns>The hediff.</returns>
        public HediffT ApplyHediff<DefT, HediffT>(string defName, BodyPart bodyPart) where DefT : HediffDef where HediffT : IHediff
        {
            DefT hediffDef = GetHediffDef<DefT>(defName);
            return (HediffT)ApplyHediff(hediffDef, bodyPart);
        }

        public IHediff ApplyHediff(string defName, BodyPart bodyPart)
        {
            HediffDef hediffDef = GetHediffDef(defName);
            return ApplyHediff(hediffDef, bodyPart);
        }

#if UNITY_EDITOR
        [ContextMenu("Print All Hediff Names")]
        public void PrintAllHediffName()
        {
            List<string> names = new();
            foreach (string name in Hediffs.Keys)
            {
                names.Add(name);
            }

            Debug.Log(string.Join(", ", names));
        }
#endif
    }
}