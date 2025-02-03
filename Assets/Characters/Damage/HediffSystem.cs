using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;
using CH.Character.Damage.Hediffs;
using CH.Character.Damage.HediffDefs;

namespace CH.Character.Damage
{
    public partial class DamageSystem : MonoBehaviour
    {
        // Constants

        /// <summary>
        /// Path to Hediff Types xml. Used for <see cref="PreloadAllHediffs"/>.
        /// </summary>
        public const string PathToHediffTypes = "Data\\DamageSystem\\Hediffs\\Hediff.xml";

        // Pre-Loading

        /// <summary>
        /// The XML document used to load in all <see cref="IDef"/>s.
        /// </summary>
        static private XmlDocument HediffDocument;

        /// <summary>
        /// Preloads all hediff types. Preferable on the awake function.
        /// </summary>
        /// <exception cref="InvalidCastException">The element type can not be converted.</exception>
        private void PreloadAllHediffs()
        {
            var hediffElements = HediffDocument.DocumentElement.ChildNodes;
            int loaded = 0;

            foreach (XmlElement hediffElem in hediffElements)
            {
                IDef hediff = hediffElem.Name switch
                {
                    "injury-hediff" => LoadInjury(hediffElem),
                    "severity-hediff" => LoadSeverityHediff(hediffElem),
                    _ => throw new InvalidCastException($"Hediff name {hediffElem.Name} is invailid"),
                };
                string keyName = hediffElem.GetAttribute("name").Replace('-', ' ');

                hediff.Name = keyName;
                Hediffs[keyName] = hediff;
                loaded++;
            }

            Debug.Log($"Loaded all {loaded} hediff(s)");
        }

        // Retrieving

        /// <summary>
        /// Gets the HediffDef by name then converts it to <typeparamref name="DefT"/> param type.
        /// </summary>
        /// <typeparam name="DefT">Needs to derive from <see cref="IDef"/>.</typeparam>
        /// <param name="name">The name of the <see cref="HediffDef"/>.</param>
        /// <returns>The HediffDef</returns>
        /// <exception cref="InvalidCastException">Thrown of the Hediff could be converted in to <typeparamref name="DefT"/>.</exception>
        public DefT GetHediffDef<DefT>(string name) where DefT : IDef
        {
            var hediff = Hediffs[name];

            if (hediff is DefT h)
                return h;
            throw new InvalidCastException($"Could't convert {name} to {typeof(DefT).FullName}");
        }

        /// <summary>
        /// All the hediff types (in name, IDef format)
        /// </summary>
        private readonly Dictionary<string, IDef> Hediffs = new();

        // Injury

        /// <summary>
        /// Injures a character's <paramref name="bodyPart"/> with a damage type
        /// </summary>
        /// <param name="damageName">The name of the damage type.</param>
        /// <param name="bodyPart">The body part affected.</param>
        /// <param name="damage">The severity of the damage.</param>
        /// <param name="isPermanent">Is the injury permanent?</param>
        /// <returns>The injury hediff applied</returns>
        public InjuryHediff InjureCharacterBP(string damageName, BodyPart bodyPart, float damage, bool isPermanent)
        {
            DamageType damageType = GetDamageTypeFromName(damageName);

            // TODO - Add Piercing Damage
            var hediff = ApplyHediff<InjuryHediffDef, InjuryHediff>(damageType.Applies, bodyPart);
            if (hediff is not InjuryHediff applied)
                throw new InvalidCastException($"Heddif {hediff} couldn't be converted into {typeof(InjuryHediff).Name}");
            applied.Severity = damage;
            applied.IsPermanent = isPermanent;
            return applied;
        }

        /// <inheritdoc cref="InjureCharacterBP(string, BodyPart, float, bool)"/>
        public InjuryHediff InjureCharacterBP(string damageName, BodyPart bodyPart, float damage)
        {
            return InjureCharacterBP(damageName, bodyPart, damage, false);
        }

        /// <summary>
        /// Loads in a injury used for <see cref="PreloadAllHediffs"/>
        /// </summary>
        /// <param name="elem">The XmlElement the def is based on</param>
        /// <returns>InjuryHediffDef</returns>
        private InjuryHediffDef LoadInjury(XmlElement elem)
        {
            InjuryHediffDef injuryHediff = new()
            {
                Bleeding = float.Parse(elem.GetNodeText("bleeding")),
                Pain = float.Parse(elem.GetNodeText("pain"))
            };

            return injuryHediff;
        }

        private SeverityHediffDef LoadSeverityHediff(XmlElement elem)
        {
            SeverityHediffDef severityHediff = new()
            {
                MaxSeverity = float.Parse(elem.GetNodeText("max-severity")),
                SeverityGain = float.Parse(elem.GetNodeText("severity-gain"))
            };

            return severityHediff;
        }

        // Applitation

        /// <inheritdoc cref="ApplyHediff(string, BodyPart)"/>
        /// <param name="hediff">A hediff defination.</param>
        public HediffT ApplyHediff<DefT, HediffT>(DefT hediff, BodyPart bodyPart) where DefT : HediffDef<HediffT> where HediffT : Hediff
        {
            HediffT applied = hediff.CreatesAppliedHediff();
            applied.AppliedTo = bodyPart;
            applied.HediffDef = hediff;
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
        /// <param name="hediffName">The name of hediff</param>
        /// <returns>The hediff.</returns>
        public HediffT ApplyHediff<DefT, HediffT>(string hediffName, BodyPart bodyPart) where DefT : HediffDef<HediffT> where HediffT : Hediff
        {
            DefT hediffDef = GetHediffDef<DefT>(hediffName);

            return ApplyHediff<DefT, HediffT>(hediffDef, bodyPart);
        }

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
    }
}