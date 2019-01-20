using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NERCore
{
    public class CRFPP
    {
       public void RunLearn(bool alternativeAlgorithm, string templateFile, string trainDataFile, string modelFile,float c)
        {
         
            string miraText = (alternativeAlgorithm) ? "-a MIRA " : string.Empty;
            Process process = new Process();
            process.StartInfo.FileName = @"crf_learn.exe";
            process.StartInfo.Arguments = $"-c {Math.Round(c, 1).ToString("0.0").Replace(",",".")} {miraText}{templateFile} {trainDataFile} {modelFile}";
            OutputText.WriteLine($"Start learn: {process.StartInfo.Arguments}");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.Default;
            process.StartInfo.StandardErrorEncoding = Encoding.Default;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += ProcessLearn_OutputDataReceived;
            process.ErrorDataReceived += ProcessLearn_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            OutputText.WriteLine($"End learn: {process.StartInfo.Arguments}");
        }

        private void ProcessLearn_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputText.WriteLine(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            testResultBuilder.AppendLine(e.Data);
        }

        private StringBuilder testResultBuilder = new StringBuilder();
        public string RunTest(string textDataFile, string modelFile)
        {

            Process process = new Process();
            process.StartInfo.FileName = @"crf_test.exe";
            process.StartInfo.Arguments = $"-m {modelFile} {textDataFile}";
            OutputText.WriteLine($"Start test: {process.StartInfo.Arguments}");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.Default;
            process.StartInfo.StandardErrorEncoding = Encoding.Default;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            OutputText.WriteLine($"End test: {process.StartInfo.Arguments}");
            return testResultBuilder.ToString();
        }



        public ResultTestCompare RunTestAndCompareToCorpus(string textDataFile, string modelFile)
        {
            string[] testLines = RunTest(textDataFile, modelFile)?.Trim()?.Split('\n');
            var result = new ResultTestCompare();
            int actualLine = 0;
            using (StreamReader reader = new StreamReader(textDataFile, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    var items = line.Split('\t');
                    if(items.Length == 6 && actualLine < testLines.Length)
                    {
                        string word = items[0];
                        string tagCorpus = items[5];
                        var splittedResult = testLines[actualLine].Split('\t');
                        if(splittedResult.Length == 7)
                        {
                            string tagCRF = splittedResult[6]?.Trim();
                            var tagResult = new TagResult() { Word = word, TagCorpus = tagCorpus, TagCRF = tagCRF };
                            if (tagCorpus!= tagCRF)
                            {
                                result.IncorrectItems.Add(tagResult);
                            }
                            else
                            {
                                result.CorrectItems.Add(tagResult);
                                if(tagCorpus!= "O")
                                {
                                    result.CorrectNotEmptyItems.Add(tagResult);
                                }
                            }
                        }
                    }
                    actualLine++;
                }
            }

            return result;
        }

    }

    public class ResultTestCompare
    {
        public int CorrectCount
        {
            get
            {
                return CorrectItems.Count;
            }
        }
        public int CorrectNotEmptyCount
        {
            get
            {
                return CorrectNotEmptyItems.Count;
            }
        }
        public int IncorrectCount
        {
            get
            {
                return IncorrectItems.Count;
            }
        }
        public List<TagResult> CorrectNotEmptyItems { get; set; } = new List<TagResult>();
        public List<TagResult> CorrectItems { get; set; } = new List<TagResult>();
        public List<TagResult> IncorrectItems { get; set; } = new List<TagResult>();
    }

    public class TagResult
    {
        public string Word { get; set; }
        public string TagCRF { get; set; }
        public string TagCorpus { get; set; }

        public override string ToString()
        {
            return $"{Word}: CRF={TagCRF} Corpus={TagCorpus}";
        }
    }





}
