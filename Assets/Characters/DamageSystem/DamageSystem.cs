using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using CH.Character.Damage.HediffDefs;
using CH.Character.Damage.Hediffs;

//? Consider - Spliting this class into seperate parts

namespace CH.Character.Damage
{
    /// <summary>
    /// The type of armour used.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="DamageSystem.DamageType"/> and armour.
    /// </remarks>
    public enum EArmourType : ushort
    {
        Sharp,
        Blunt,
        Flame,
        Blast,
        Chemical,
        None
    }

    /// <summary>
    /// Controls all things about damaging and applying hediffs
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        // Classes and Structs

        /// <summary>
        /// A definition of damage type.
        /// Controls how it applies damage.
        /// </summary>
        public class DamageType
        {
            /// <summary>
            /// Name of the DamageType (of the format <c>foo bar</c>)
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// Does this type pierce through parent body parts when attacking a child.
            /// </summary>
            public readonly bool Piercing;
            /// <summary>
            /// How long does it stun a character (in seconds).
            /// </summary>
            public readonly float StunTime;
            /// <summary>
            /// The armour type that this targets.
            /// </summary>
            public readonly EArmourType ArmourType;
            /// <summary>
            /// Applies this damage type on hit.
            /// </summary>
            public readonly InjuryHediffDef Applies;
            /// <summary>
            /// Applies this damage type on piercing.
            /// </summary>
            /// <remarks>
            /// May be <c>null</c> when <see cref="Piercing"/> is <c>false</c>.
            /// </remarks>
            public readonly InjuryHediffDef AppliesPiercing;

            public override string ToString()
            {
                return Name;
            }

            internal DamageType(string name, bool piercing, float stunTime, EArmourType armourType, InjuryHediffDef applies, InjuryHediffDef appliesPiercing)
            {
                Name = name;
                Piercing = piercing;
                StunTime = stunTime;
                ArmourType = armourType;
                Applies = applies;
                AppliesPiercing = appliesPiercing;
            }
        }

        // Constants

        /// <summary>
        /// Path to Damage Type xml. Used for <see cref="PreloadAllDamageTypes"/>
        /// </summary>
        public const string PathToDamageTypesXML = "Data\\DamageSystem\\DamageType\\DamageType.xml";

        /// <summary>
        /// Path to Hediff Types xml. Used for <see cref="PreloadAllHediffs"/>.
        /// </summary>
        public const string PathToHediffTypes = "Data\\DamageSystem\\Hediffs\\Hediff.xml";

        /// <summary>
        /// The tag of the <c>DamageSystem</c> Gameobject.
        /// </summary>
        public const string DamageSysTag = "DamageSystem";

        // Singleton

        /// <summary>
        /// The cached <c>DamageSystem</c> component retrieved from <see cref="GetDamageSystem"/>.
        /// </summary>
        static private DamageSystem _CachedDamageSystem;

        /// <summary>
        /// Get the <c>DamageSystem</c> component.
        /// </summary>
        /// <returns>The <c>DamageSystem</c> component</returns>
        /// <exception cref="NullReferenceException">Thrown when the damage system object or the <c>DamageSystem</c> component wasn't found.</exception>
        static public DamageSystem GetDamageSystem()
        {
            if (_CachedDamageSystem != null)
                return _CachedDamageSystem;
            GameObject damageObj = GameObject.FindGameObjectWithTag(DamageSysTag);
            if (damageObj == null)
                throw new NullReferenceException("Damage System not found by tag!");
            if (!damageObj.TryGetComponent<DamageSystem>(out var damageSys))
                throw new NullReferenceException("Damage System Script not found!");
            _CachedDamageSystem = damageSys;
            return damageSys;
        }

        // Pre-Loading

        // - Damage Type

        /// <summary>
        /// Converts a XML element to a <see cref="DamageType"/>.
        /// </summary>
        /// <param name="element">XMLElement</param>
        /// <returns>A DamageType based on the <paramref name="element"/> passed.</returns>
        /// <exception cref="InvalidCastException">The armour category couldn't be converted <see cref="EArmourType"/>.</exception>
        private DamageType ConvertXmlToType(XmlElement element)
        {
            string name = element.GetAttribute("name"),
            armourCat = element.GetNodeText("armour-category");
            bool piercing = bool.Parse(element.GetNodeText("piercing"));
            float stunTime = float.Parse(element.GetNodeText("stun-time"));
            InjuryHediffDef applies = GetHediffDef<InjuryHediffDef>(element.GetNodeText("applies")),
            appliesPiercing = null;

            if (piercing)
                appliesPiercing = GetHediffDef<InjuryHediffDef>(element.GetNodeText("applies-piercing"));

            if (!Enum.TryParse<EArmourType>(armourCat, out var armourType))
                throw new InvalidCastException($"Tried to convert {armourCat} to an armour type");
            return new DamageType(name, piercing, stunTime, armourType, applies, appliesPiercing);
        }

        /// <summary>
        /// The XML document used to load in all <see cref="DamageType"/>
        /// </summary>
        static private XmlDocument TypesDocument;

        /// <summary>
        /// Preloads all damage types. Preferable on the awake function.
        /// </summary>
        private void PreloadAllDamageTypes()
        {
            var typesElements = TypesDocument.DocumentElement.ChildNodes;
            int loaded = 0;

            foreach (XmlElement typeElem in typesElements)
            {
                DamageType damageType = ConvertXmlToType(typeElem);
                DamageTypes.Add(damageType.Name, damageType);
                loaded++;
            }

            Debug.Log($"Loaded all {loaded} damage types(s)");
        }

        // - Hediff

        /// <summary>
        /// The XML document used to load in all <see cref="IHediffDef"/>s.
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
                    _ => throw new InvalidCastException($"Hediff name {hediffElem.Name} is invailid"),
                };
                string keyName = hediffElem.GetAttribute("name").Replace('-', ' ');

                Hediffs[keyName] = hediff;
                loaded++;
            }

            Debug.Log($"Loaded all {loaded} hediff(s)");
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

        // Damage Types

        /// <summary>
        /// All the damage types (in name, DamageType format).
        /// </summary>
        private readonly Dictionary<string, DamageType> DamageTypes = new();
        /// <summary>
        /// Get the damage type by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the damage type</param>
        /// <returns>DamageType</returns>
        public DamageType GetDamageTypeFromName(string name)
        {
            return DamageTypes[name];
        }

        // Hediffs

        /// <summary>
        /// All the hediff types (in name, IDef format)
        /// </summary>
        private readonly Dictionary<string, IDef> Hediffs = new();

        /// <summary>
        /// Injures a character's <paramref name="bodyPart"/> with a damage type
        /// </summary>
        /// <param name="damageName">The name of the damage type.</param>
        /// <param name="bodyPart">The body part affected.</param>
        /// <param name="damage">The severity of the damage.</param>
        /// <param name="isPermanent">Is the injury permanent?</param>
        /// <returns>The injury hediff applied</returns>
        public InjuryHediff InjureCharacterBP(string damageName, CharacterBP bodyPart, float damage, bool isPermanent)
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

        /// <inheritdoc cref="InjureCharacterBP(string, CharacterBP, float, bool)"/>
        public InjuryHediff InjureCharacterBP(string damageName, CharacterBP bodyPart, float damage)
        {
            return InjureCharacterBP(damageName, bodyPart, damage, false);
        }

        /// <summary>
        /// Gets the HediffDef by name then converts it to <typeparamref name="HediffT"/> param type.
        /// </summary>
        /// <typeparam name="HediffT">Needs to derive from <see cref="IDef"/>.</typeparam>
        /// <param name="name">The name of the <see cref="HediffDef"/>.</param>
        /// <returns>The HediffDef</returns>
        /// <exception cref="InvalidCastException">Thrown of the Hediff could be converted in to <typeparamref name="HediffT"/>.</exception>
        public HediffT GetHediffDef<HediffT>(string name) where HediffT : IDef
        {
            var hediff = Hediffs[name];

            if (hediff is HediffT h)
                return h;
            throw new InvalidCastException($"Could't convert {name} to {typeof(HediffT).FullName}");
        }

        /// <summary>
        /// Applies a hediff to a bodypart.
        /// </summary>
        /// <param name="hediff">A hediff defination.</param>
        /// <param name="bodyPart">The bodypart, the hediff is going to be applied on.</param>
        /// <returns>The hediff.</returns>
        public Hediff ApplyHediff<DefT, HediffT>(DefT hediff, CharacterBP bodyPart) where DefT : HediffDef<HediffT> where HediffT : Hediff
        {
            Hediff applied = hediff.CreatesAppliedHediff();
            applied.AppliedTo = bodyPart;
            applied.OnApplied();
            return applied;
        }

        // Unity

        public void Awake()
        {
            HediffDocument = new();
            HediffDocument.Load(PathToHediffTypes);

            TypesDocument = new();
            TypesDocument.Load(PathToDamageTypesXML);

            PreloadAllHediffs();
            PreloadAllDamageTypes();
        }
    }
}