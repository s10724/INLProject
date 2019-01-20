using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusNamedDocument : CorpusItem
    {
        public static readonly Dictionary<string, string> AllowedShortTagNames = new Dictionary<string, string>
        {
            {"B-persName", "B-PER" },
            {"I-persName", "I-PER" },
            {"B-placeName", "B-LOC" },
            {"B-geogName", "B-GEO" },
            {"I-placeName", "I-LOC" },
            {"I-geogName", "I-GEO" },
            {"B-orgName", "B-ORG" },
            {"I-orgName", "I-ORG" },
            {"O", "O" },
        };

        public List<CorpusNamedAnnotation> NamedAnnotations { get; set; } = new List<CorpusNamedAnnotation>();

        public void FillData(string xmlContent, string fileName)
        {
            Id = fileName;
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlContent);
            XmlNodeList nodes = xdoc.GetElementsByTagName("seg");

            foreach (XmlNode nodeS in nodes)
            {
                var namedAnnotation = new CorpusNamedAnnotation();
                namedAnnotation.FillData(nodeS);
                NamedAnnotations.Add(namedAnnotation);
            }
            FillWordTargetIds();
        }

        private void FillWordTargetIds()
        {
            foreach (CorpusNamedAnnotation namedAnnotation in NamedAnnotations)
            {
                foreach (var target in namedAnnotation.TargetIds)
                {
                    AddNewTarget(target, namedAnnotation);
                }
            }
        }

        private void AddNewTarget(string target, CorpusNamedAnnotation namedAnnotation)
        {
            if (target.StartsWith("named"))
            {
                FindAllWordsFromTarget(target, namedAnnotation);
            }
            else
            {
                namedAnnotation.WordTargetIds.Add(target.Replace("ann_morphosyntax.xml#", ""));
            }
        }

        private void FindAllWordsFromTarget(string target, CorpusNamedAnnotation namedAnnotation)
        {
            var listOfAnnotation = NamedAnnotations.Where(x => x.Id == target).SelectMany(x => x.TargetIds).ToList();
            foreach (var newTarget in listOfAnnotation)
            {
                AddNewTarget(newTarget, namedAnnotation);
            }
        }


        public void FillTypeNameIOB2TagsInWords(List<CorpusWord> words)
        {
            foreach (CorpusNamedAnnotation namedAnnotation in NamedAnnotations)
            {
                for (int i = 0; i < namedAnnotation.WordTargetIds.Count; i++)
                {
                    string IOB2Prefix = (i == 0) ? "B" : "I";
                    var word = words.Where(x => x.Id == namedAnnotation.WordTargetIds[i]).Single();
                    string tag = $"{IOB2Prefix}-{namedAnnotation.TypeName}";
                    if (AllowedShortTagNames.ContainsKey(tag))
                    {
                        //if (!string.IsNullOrWhiteSpace(namedAnnotation.SubtypeName))
                        //{
                        //    tag += $":{namedAnnotation.SubtypeName}";
                        //}
                        word.TypeNameIOB2Tags.Add(tag);
                    }
                }
            }
        }

    }
}
