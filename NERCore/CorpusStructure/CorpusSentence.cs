using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusSentence : CorpusItem
    {
        public List<CorpusWord> Words { get; set; } = new List<CorpusWord>();

        public void FillData(XmlNode nodeS)
        {
            Id = nodeS.Attributes["xml:id"].Value;
            var nodes = nodeS.ChildNodes.OfType<XmlElement>();
            foreach (XmlNode nodeW in nodes)
            {
                var word = new CorpusWord();
                word.FillData(nodeW);
                Words.Add(word);
            }
        }
    }
}
