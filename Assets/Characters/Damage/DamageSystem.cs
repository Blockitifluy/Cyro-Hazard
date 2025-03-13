using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CyroHazard.Character.Damage
{
    /// <summary>
    /// The type of armour used.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="DamageSystem.DamageType"/> and armour.
    /// </remarks>
    public enum EArmourType : ushort
    {
        [XmlEnum("Sharp")]
        Sharp,
        [XmlEnum("Blunt")]
        Blunt,
        [XmlEnum("Flame")]
        Flame,
        [XmlEnum("Blast")]
        Blast,
        [XmlEnum("Chemical")]
        Chemical,
        [XmlEnum("None")]
        None
    }

    /// <summary>
    /// Controls all things about damaging and applying hediffs
    /// </summary>
    public partial class DamageSystem : MonoBehaviour
    {
        // Classes and Structs

        /// <summary>
        /// A definition of damage type.
        /// Controls how it applies damage.
        /// </summary>
        [XmlRoot("damage-type"), XmlType("damage-type")]
        public class DamageType
        {
            /// <summary>
            /// Name of the DamageType (of the format <c>foo bar</c>)
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }
            /// <summary>
            /// Does this type pierce through parent body parts when attacking a child.
            /// </summary>
            [XmlElement("piercing")]
            public bool Piercing { get; set; }
            /// <summary>
            /// How long does it stun a character (in seconds).
            /// </summary>
            [XmlElement("stun-time")]
            public float StunTime { get; set; }
            /// <summary>
            /// The armour type that this targets.
            /// </summary>
            [XmlElement("armour-category")]
            public EArmourType ArmourType { get; set; }
            /// <summary>
            /// Applies this damage type on hit.
            /// </summary>
            [XmlElement("applies")]
            public string Applies { get; set; }
            /// <summary>
            /// Applies this damage type on piercing.
            /// </summary>
            /// <remarks>
            /// May be <c>null</c> when <see cref="Piercing"/> is <c>false</c>.
            /// </remarks>
            [XmlElement("applies-piercing", IsNullable = true)]
            public string AppliesPiercing { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public DamageType() { }
        }

        // Constants

        /// <summary>
        /// Path to Damage Type xml. Used for <see cref="PreloadAllDamageTypes"/>
        /// </summary>
        public const string PathToDamageTypesXML = "Data\\DamageSystem\\DamageType\\DamageType.xml";

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

        // Retrieving

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

        // Pre-Loading

        /// <summary>
        /// Converts a XML element to a <see cref="DamageType"/>.
        /// </summary>
        /// <param name="element">XMLElement</param>
        /// <returns>A DamageType based on the <paramref name="element"/> passed.</returns>
        /// <exception cref="InvalidCastException">The armour category couldn't be converted <see cref="EArmourType"/>.</exception>
        private DamageType ConvertXmlToType(XmlElement element)
        {
            XmlSerializer serializer = new(typeof(DamageType));

            using StringReader reader = new(element.OuterXml);
            DamageType damageType = (DamageType)serializer.Deserialize(reader);

            return damageType;
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


#if UNITY_EDITOR
        [ContextMenu("Print All DamageType Names")]
        public void PrintAllDTName()
        {
            List<string> names = new();
            foreach (string name in DamageTypes.Keys)
            {
                names.Add(name);
            }

            Debug.Log(string.Join(", ", names));
        }
#endif

        // Unity

        public void Awake()
        {
            HediffDocument = new();
            HediffDocument.Load(Application.streamingAssetsPath + "\\" + PathToHediffTypes);

            TypesDocument = new();
            TypesDocument.Load(Application.streamingAssetsPath + "\\" + PathToDamageTypesXML);

            PreloadHediffTypes();
            PreloadHediffs();

            PreloadAllDamageTypes();
        }
    }
}