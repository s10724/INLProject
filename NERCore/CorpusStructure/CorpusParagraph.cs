using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusParagraph : CorpusItem
    {
        public List<CorpusSentence> Sentences { get; set; } = new List<CorpusSentence>();

        public void FillData(XmlNode nodeP)
        {
            Id = nodeP.Attributes["xml:id"].Value;
            var nodes = nodeP.ChildNodes.OfType<XmlElement>();
            foreach (XmlNode nodeS in nodes)
            {
                var sentence = new CorpusSentence();
                sentence.FillData(nodeS);
                Sentences.Add(sentence);
            }
        }
    }
}
