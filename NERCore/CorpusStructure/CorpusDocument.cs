using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusDocument : CorpusItem
    {
        public List<CorpusParagraph> Paragraphs { get; set; } = new List<CorpusParagraph>();

        public void FillData(string xmlContent, string fileName)
        {
            Id = fileName;
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlContent);
            XmlNodeList nodes = xdoc.GetElementsByTagName("p");

            foreach (XmlNode nodeP in nodes)
            {
                var paragraph = new CorpusParagraph();
                paragraph.FillData(nodeP);
                Paragraphs.Add(paragraph);
            }
        }
    }
}
