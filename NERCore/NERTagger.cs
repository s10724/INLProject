using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NERCore
{
    public class NERTagger : ConcraftProvider
    {
        public List<TaggerResult> TagText(string text, string modelFile)
        {
            List<TaggerResult> results = new List<TaggerResult>();
            CRFPP crf = new CRFPP();
            string fileName = $@"temp\{DateTime.Now.Ticks}.data";
            Directory.CreateDirectory("temp");
            string[] concraftLines = Run(text, fileName)?.Trim()?.Split('\n');
            string[] crfLines = crf.RunTest(fileName, modelFile)?.Trim()?.Split('\n');
            int indexStart = -1;
            int indexEnd = 0;
            string tagShort = string.Empty;
            if (concraftLines.Length == crfLines.Length)
            {
                void AddNewPart()
                {
                    int length = indexEnd - indexStart;
                    results.Add(new TaggerResult()
                    {
                        Sentence = text.Substring(indexStart, length),
                        Tag = tagShort,
                        StartIndex = indexStart,
                        EndIndex = indexEnd
                    });
                }

                for (int i = 0; i < concraftLines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(concraftLines[i]))
                    {
                        var word = new CorpusWord();
                        word.SetDataFromConcraftLine(concraftLines[i]?.Trim());
                        word.SetTagFromCrfLine(crfLines[i]?.Trim());
                        string tag = word.FirstTypeNameIOB2Tags;

                        if (tag[0] == 'B')
                        {
                            if (indexStart >= 0)
                            {
                                AddNewPart();
                            }
                            indexStart = text.IndexOf(word.Text, indexEnd);
                            indexEnd = indexStart + word.Text.Length;
                            var splitted = tag.Split('-');
                            tagShort = splitted.Length == 2 ? splitted[1] : string.Empty;
                        }
                        else if (tag[0] == 'I')
                        {
                            indexEnd = text.IndexOf(word.Text, indexEnd) + word.Text.Length;
                        }
                        else
                        {
                            if (indexStart >= 0)
                            {
                                AddNewPart();
                            }
                            indexStart = -1;
                        }
                    }
                }
                if (indexStart >= 0)
                {
                    AddNewPart();
                }
            }



            for (int i = 0; i < results.Count - 1; i++)
            {
                var partActual = results[i];
                var partNext = results[i + 1];
                if(partActual.EndIndex!= partNext.StartIndex)
                {
                    int startIndex = partActual.EndIndex;
                    int endIndex = partNext.StartIndex;
                    int length = endIndex - startIndex;
                    results.Insert(results.IndexOf(partActual)+1, new TaggerResult()
                    {
                        Sentence = text.Substring(startIndex, length),
                        Tag = "",
                        StartIndex = startIndex,
                        EndIndex = endIndex
                    });
                    i++;
                }
              
            }

            if (results.Count > 0)
            {
                if (results[0].StartIndex != 0)
                {
                    int endIndex = results[0].StartIndex;
                    results.Insert(0, new TaggerResult()
                    {
                        Sentence = text.Substring(0, endIndex),
                        Tag = "",
                        StartIndex = 0,
                        EndIndex = endIndex
                    });
                }
            }

            if (results.Count > 0)
            {
                if (results[0].EndIndex != text.Length-1)
                {
                    int startIndex = results[results.Count-1].EndIndex;
                    int endIndex = text.Length;
                    int length = endIndex - startIndex;
                    results.Insert(results.Count, new TaggerResult()
                    {
                        Sentence = text.Substring(startIndex, length),
                        Tag = "",
                        StartIndex = startIndex,
                        EndIndex = endIndex
                    });
                }
            }
            return results;
        }

    }

    public class TaggerResult
    {
        public string Sentence { get; set; }
        public string Tag { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public override string ToString()
        {
            return $"{Sentence} [{Tag}]";
        }
    }
}
