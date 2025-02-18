using System.Xml;
using CH.Character.Damage.HediffDefs;
using UnityEngine;

namespace CH.Character.Damage.DefParser
{
    [CreateAssetMenu(fileName = "SeverityDefParser", menuName = "Damage System/Parser/SeverityDefParser")]
    public class SeverityDefParser : DefParser
    {
        public override string GetTypeName()
        {
            return "severity-hediff";
        }

        public override IDef Load(XmlElement elem)
        {
            SeverityHediffDef severityHediff = new()
            {
                MaxSeverity = float.Parse(elem.GetNodeText("max-severity")),
                SeverityGain = float.Parse(elem.GetNodeText("severity-gain"))
            };

            return severityHediff;
        }
    }
}