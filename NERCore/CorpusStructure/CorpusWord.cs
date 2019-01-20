using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NERCore
{
    public class CorpusWord : CorpusItem
    {
        public static readonly char Sep = '\t';
        public string Text { get; set; }
        public string Base { get; set; }
        public string CTag { get; set; }
        public string Msd { get; set; }
        public bool IsStartWithCapitalLetter { get; set; }
        public List<string> TypeNameIOB2Tags { get; set; } = new List<string>();

        public string FirstTypeNameIOB2Tags
        {
            get
            {
                return TypeNameIOB2Tags.FirstOrDefault();
            }
        }

        public string GetWordLine()
        {
            string IsStartWithCapitalLetterStr = (IsStartWithCapitalLetter) ? "W" : "O";
            string data = $"{NormalizeData(Text)}{Sep}{NormalizeData(Base, NormalizeData(Text))}{Sep}{IsStartWithCapitalLetterStr}{Sep}{NormalizeData(CTag)}{Sep}{NormalizeData(Msd)}{Sep}{NormalizeData(FirstTypeNameIOB2Tags)}\n";

            if (!ValidateData(data))
                OutputText.WriteLine($"Nie poprawna liczba danych w pozycji: {data}");

            return data;
        }

        public void SetDataFromConcraftLine(string line)
        {
            string[] splittedLine = line.Split(' ');
            if (splittedLine.Length == 5)
            {
                Text =  splittedLine[0];
                Base = splittedLine[1];
                IsStartWithCapitalLetter = splittedLine[2] == "W";
                CTag = splittedLine[3];
                Msd = splittedLine[4];
            }
        }

        public void SetTagFromCrfLine(string line)
        {
            string[] splittedLine = line.Split('\t');
            if(splittedLine.Length == 6)
            {
                TypeNameIOB2Tags = new List<string>() { splittedLine[5] };
            }
        }

        public string NormalizeData(string value,string valueIfEmpty = "O")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return valueIfEmpty;
            }

            if (value.Contains(" "))
                value = value.Replace(" ", "");

            if (value.Contains("\t"))
                value = value.Replace(" ", "\t");

            return value.Trim();
        }

        public void FillData(XmlNode nodeW)
        {
            Id = nodeW.Attributes["xml:id"].Value;
            var nodes = nodeW.ChildNodes;
            foreach (XmlNode fs in nodes)
            {
                var fsNodes = fs.ChildNodes.OfType<XmlElement>();

                this.Text = fsNodes.Where(e => e.Attributes["name"]?.Value == "orth").FirstOrDefault()?.FirstChild.InnerText;
                var disamb = fsNodes.Where(e => e.Attributes["name"]?.Value == "disamb").FirstOrDefault()?
                    .FirstChild.ChildNodes.OfType<XmlElement>()
                    .Where(y => y.Attributes["name"]?.Value == "interpretation").FirstOrDefault()?
                    .FirstChild.InnerText;

                IsStartWithCapitalLetter = char.IsUpper(Text?.FirstOrDefault() ?? 'a');

                if (disamb != null)
                {
                    var segment = disamb.Split(':');
                    if (segment.Length > 0)
                    {
                        Base = segment[0];
                        if (!string.IsNullOrWhiteSpace(Base))
                        {
                            IsStartWithCapitalLetter = char.IsUpper(Base.FirstOrDefault());
                        }
                    }
                    if (segment.Length > 1)
                    {
                        CTag = segment[1];
                    }
                    if (segment.Length > 2)
                    {
                        Msd = string.Join(":", segment.Skip(2));
                    }
                }
            }
        }

        private bool ValidateData(string data)
        {
            var splittedLine = data.Split(Sep);
            return (splittedLine.Length == 6);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
