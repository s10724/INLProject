using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusNamedAnnotation : CorpusItem
    {
        public string TypeName { get; set; }
        public string SubtypeName { get; set; }
        public List<string> WordTargetIds { get; set; } = new List<string>();
        public List<string> TargetIds { get; set; } = new List<string>();

        public void FillData(XmlNode nodeS)
        {
            Id = nodeS.Attributes["xml:id"].Value;
            var qNodes = nodeS.ChildNodes.OfType<XmlElement>();
            var fs = qNodes.Where(e => e.Name == "fs" && e.Attributes["type"]?.Value == "named").FirstOrDefault()?.ChildNodes.OfType<XmlElement>();
            TypeName = fs.Where(e => e.Attributes["name"]?.Value == "type").FirstOrDefault()?.FirstChild.Attributes["value"].Value;
            SubtypeName = fs.Where(e => e.Attributes["name"]?.Value == "subtype").FirstOrDefault()?.FirstChild.Attributes["value"].Value;
            var ptrList = qNodes.Where(e => e.Name == "ptr").ToList();

            foreach (var ptr in ptrList)
            {
                TargetIds.Add(ptr.Attributes["target"].Value);
            }
        }


    }
}
