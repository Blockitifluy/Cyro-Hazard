using System.Xml;
using CH.Character.Damage.HediffDefs;
using UnityEngine;

namespace CH.Character.Damage.DefParser
{
    [CreateAssetMenu(fileName = "InjuryDefParser", menuName = "Damage System/Parser/InjuryDefParser")]
    public class InjuryDefParser : DefParser
    {
        public override string GetTypeName()
        {
            return "injury-hediff";
        }

        public override IDef Load(XmlElement elem)
        {
            InjuryHediffDef injuryHediff = new()
            {
                Bleeding = float.Parse(elem.GetNodeText("bleeding")),
                Pain = float.Parse(elem.GetNodeText("pain"))
            };

            return injuryHediff;
        }
    }
}