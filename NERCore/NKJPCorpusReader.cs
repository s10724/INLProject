using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Linq;

namespace NERCore
{
    public class NKJPCorpusReader
    {
        public string CorpusArchiveFile { get; set; }
        public string CorpusOutputFileName { get; set; }

        public NKJPCorpusReader(string CorpusArchiveFile, string CorpusOutputFileName)
        {
            this.CorpusArchiveFile = CorpusArchiveFile;
            this.CorpusOutputFileName = CorpusOutputFileName;
        }

        private string ReadTarFile(TarInputStream tarStr)
        {
            string xmlContent = string.Empty;
            using (Stream sr = new MemoryStream())
            {
                tarStr.CopyEntryContents(sr);
                sr.Position = 0;
                xmlContent = new StreamReader(sr).ReadToEnd();
            }
            return xmlContent;
        }
        private int ActualFolderInCorpus = 0;

        public void ExtractSentencesWithNamedWords()
        {
            ClearOutputFile();
            OutputText.WriteLine("Rozpoczęto przetwarzanie archiwum!");
            using (FileStream fs = new FileStream(CorpusArchiveFile, FileMode.Open))
            using (Stream source = new GZipInputStream(fs))
            {
                using (TarInputStream tarStr = new TarInputStream(source))
                {
                    string actualFolder = string.Empty;
                    string morphXml = string.Empty;
                    string namedXml = string.Empty;
                    TarEntry te;
                    while ((te = tarStr.GetNextEntry()) != null)
                    {
                        if (te.IsDirectory)
                        {
                           
                            AnalyzeCorpusSegment(morphXml, namedXml, actualFolder);
                            actualFolder = te.Name;
                            morphXml = namedXml = string.Empty;
                        }
                        else
                        {
                            if (te.Name == ($"{actualFolder}ann_morphosyntax.xml"))
                            {
                                morphXml = ReadTarFile(tarStr);
                            }
                            else if (te.Name == ($"{actualFolder}ann_named.xml"))
                            {
                                namedXml = ReadTarFile(tarStr);
                            }
                        }
                    }
                    AnalyzeCorpusSegment(morphXml, namedXml, actualFolder);
                }
            }

            OutputText.WriteLine("Zakończono przetwarzanie archiwum!");







        }
        private int NumberOfLineInCorpusExportedFile = 0;
        public void ReadNumberOfLineInCorpusExportedFile()
        {
            NumberOfLineInCorpusExportedFile = 0;
            using (StreamReader reader = new StreamReader(CorpusOutputFileName, true))
            {
                while(!reader.EndOfStream)
                {
                    reader.ReadLine();
                    NumberOfLineInCorpusExportedFile++;
                }
            }
        }

        public void SaveCorpusToFile(int percentSkip=0, int percentTake = 100)
        {
            OutputText.WriteLine($"Rozpoczęto podział korpusu: Od {percentSkip}% do {percentSkip+ percentTake}%!"); 
            bool isEndSkip = (percentSkip == 0);

            int skipLines = (int)Math.Round(((double)percentSkip * (double)NumberOfLineInCorpusExportedFile) / 100.0);
            int takeLines = (int)Math.Round((((double)percentSkip + (double)percentTake) * (double)NumberOfLineInCorpusExportedFile) / 100.0);


            int numberOfLine = 0;
            using (StreamReader reader = new StreamReader(CorpusOutputFileName,true))
            {
                using (StreamWriter writer = new StreamWriter($"{CorpusOutputFileName}({percentSkip}-{percentSkip+percentTake})"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (isEndSkip)
                        {
                            writer.Write($"{line}\n");
                        }

                        if (line == "")
                        {
                           if (numberOfLine> skipLines)
                           {
                                isEndSkip = true;
                           }

                           if (numberOfLine > takeLines)
                           {
                                break;
                           }
                        }


                        numberOfLine++;
                    }
                }
            }
            OutputText.WriteLine($"Zakończono podział korpusu: Od {percentSkip}% do {percentSkip + percentTake}%!\r\n");
        }


        private void ClearOutputFile()
        {
            var myFile = File.Create(CorpusOutputFileName);
            myFile.Close();
        }

        private void SaveContentToFile(CorpusDocument doc)
        {
            using (StreamWriter writer = new StreamWriter(CorpusOutputFileName, true))
            {
                foreach(var p in doc.Paragraphs)
                {
                    foreach (var s in p.Sentences)
                    {
                        foreach (var w in s.Words)
                        {
                            writer.Write(w.GetWordLine());
                        }
                        writer.Write("\n");
                    }
                }
            }
        }

        private void AnalyzeCorpusSegment(string morphXml, string namedXml, string actualFolder)
        {
            ActualFolderInCorpus++;
            if (!string.IsNullOrWhiteSpace(morphXml))
            {
                OutputText.WriteLine($"Ekstracja i tagowanie słów z folderu [{ActualFolderInCorpus}]: {actualFolder}");
                var doc = new CorpusDocument();
                doc.FillData(morphXml, actualFolder);

                if (!string.IsNullOrWhiteSpace(namedXml))
                {
                    var docNamed = new CorpusNamedDocument();
                    docNamed.FillData(namedXml, actualFolder);

                    var words = doc.Paragraphs.SelectMany(x => x.Sentences).SelectMany(x => x.Words).ToList();
                    docNamed.FillTypeNameIOB2TagsInWords(words);
                }
                SaveContentToFile(doc);
            }
        }
    }
}
