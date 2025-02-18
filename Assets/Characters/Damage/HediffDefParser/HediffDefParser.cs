using System.Xml;
using CH.Character.Damage.HediffDefs;
using UnityEngine;

namespace CH.Character.Damage.DefParser
{
    public abstract class DefParser : ScriptableObject
    {
        public abstract string GetTypeName();

        public abstract IDef Load(XmlElement elem);
    }
}