using CommandLine;
using NERCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NERUtilitiesConsoleApp
{
    class Program
    {
        class LearnBase
        {
            [Option('a', "mira", Required = false, HelpText = "Use MIRA algorithm.")]
            public bool UseMIRA { get; set; }
            [Option('s', "source", Required = true, HelpText = "Extracted corpus file in text format.")]
            public string CorpusSourceFile { get; set; }
            [Option('m', "model", Required = true, HelpText = "Computed model file.")]
            public string ModelOutputFile { get; set; }
            [Option('t', "template", Required = true, HelpText = "Template file.")]
            public string TemplateFile { get; set; }
            [Option('c', "cparam", Required = true, HelpText = "Balance between overfitting and underfitting.")]
            public float CValue { get; set; }
        }

        [Verb("test", HelpText = "Learn and test crf model from corpus text file")]
        class TestOptions : LearnBase
        {
            [Option('p', "pattern", Required = true, HelpText = "Test corpus file in text format.")]
            public string CorpusTestFile { get; set; }
            [Option('r', "report", Required = false, HelpText = "Generate report to file.")]
            public bool GenerateReport { get; set; }
        }

        [Verb("report", HelpText = "Test crf model from corpus text file")]
        class ReportOptions
        {
            [Option('m', "model", Required = true, HelpText = "Computed model file.")]
            public string ModelOutputFile { get; set; }
            [Option('p', "pattern", Required = true, HelpText = "Test corpus file in text format.")]
            public string CorpusTestFile { get; set; }
            [Option('r', "report", Required = false, HelpText = "Generate report to file.")]
            public bool GenerateReport { get; set; }
        }

        [Verb("learn", HelpText = "Learn crf model from corpus text file")]
        class LearnOptions: LearnBase
        {

        }

        [Verb("extract", HelpText = "Extract NKJP .tar.gz subcorpus to tagged corpus text file")]
        class ExtractOptions
        {
            [Option('s', "source", Required = true, HelpText = "Corpus file to be processed in .tar.gz format.")]
            public string NKJPSourceFile { get; set; }
            [Option('o', "output", Required = true, HelpText = "Exported corpus file in text format.")]
            public string CorpusOutputFile { get; set; }
        }

        [Verb("split", HelpText = "Split tagged corpus text file.")]
        class SplitOptions
        {
            [Option('s', "source", Required = true, HelpText = "Extracted corpus file in text format.")]
            public string CorpusSourceFile { get; set; }
            [Option('o', "offset", Required = true, HelpText = "Position start in percent of splitted file")]
            public int PercentOfFileStart { get; set; }
            [Option('l', "length", Required = true, HelpText = "Length in percent of splitted file")]
            public int PercentOfFile { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ExtractOptions, SplitOptions, LearnOptions, TestOptions>(args)
                 .WithParsed<ExtractOptions>(o =>
                 {
                     NKJPCorpusReader reader = new NKJPCorpusReader(o.NKJPSourceFile, o.CorpusOutputFile);
                     reader.ExtractSentencesWithNamedWords();
                 })
                .WithParsed<SplitOptions>(o =>
                {
                    if (o.PercentOfFile < 1 || o.PercentOfFile + o.PercentOfFileStart > 100)
                    {
                        OutputText.WriteLine($"Incorrect length in percent!");
                        return;
                    }
                    NKJPCorpusReader reader = new NKJPCorpusReader(null, o.CorpusSourceFile);
                    reader.ReadNumberOfLineInCorpusExportedFile();
                    reader.SaveCorpusToFile(o.PercentOfFileStart, o.PercentOfFile);
                })
                .WithParsed<LearnOptions>(o =>
                {
                    var crf = new CRFPP();
                    crf.RunLearn(o.UseMIRA, o.TemplateFile, o.CorpusSourceFile, o.ModelOutputFile, o.CValue);
                })
                .WithParsed<TestOptions>(o =>
                {
                    if (o.GenerateReport)
                    {
                        DirectoryInfo di = Directory.CreateDirectory("reports");
                        string fileReport = $@"reports/report_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt";
                        OutputText.WriteLine($"Save report to file: {fileReport}");
                        OutputText.FileOutput = fileReport;
                    }
                    var crf = new CRFPP();
                    OutputText.WriteLine($"CRF++ template:\r\n{File.ReadAllText(o.TemplateFile)}\r\n");
                    crf.RunLearn(o.UseMIRA, o.TemplateFile, o.CorpusSourceFile, o.ModelOutputFile,o.CValue);
                    var result = crf.RunTestAndCompareToCorpus(o.CorpusTestFile, o.ModelOutputFile);
                    double correctPercent = Math.Round(((double)result.CorrectCount / ((double)result.CorrectCount + (double)result.IncorrectCount)) * 100.0, 2);
                    double correctNoEmptyPercent = Math.Round(((double)result.CorrectNotEmptyCount / ((double)result.CorrectNotEmptyCount + (double)result.IncorrectCount)) * 100.0, 2);
                    OutputText.WriteLine($"Results:\r\nCorrect: {result.CorrectCount}({correctPercent}%)\r\nCorrect not empty:{result.CorrectNotEmptyCount}({correctNoEmptyPercent}%)\r\nIncorrect: {result.IncorrectCount}");
                    OutputText.FileOutput = "";
                })
                .WithParsed<ReportOptions>(o =>
                {
                    if (o.GenerateReport)
                    {
                        DirectoryInfo di = Directory.CreateDirectory("reports");
                        string fileReport = $@"reports/report_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt";
                        OutputText.WriteLine($"Save report to file: {fileReport}");
                        OutputText.FileOutput = fileReport;
                    }
                    var crf = new CRFPP();
                    var result = crf.RunTestAndCompareToCorpus(o.CorpusTestFile, o.ModelOutputFile);
                    double correctPercent = Math.Round(((double)result.CorrectCount / ((double)result.CorrectCount + (double)result.IncorrectCount)) * 100.0, 2);
                    double correctNoEmptyPercent = Math.Round(((double)result.CorrectNotEmptyCount / ((double)result.CorrectNotEmptyCount + (double)result.IncorrectCount)) * 100.0, 2);
                    OutputText.WriteLine($"Results:\r\nCorrect: {result.CorrectCount}({correctPercent}%)\r\nCorrect not empty:{result.CorrectNotEmptyCount}({correctNoEmptyPercent}%)\r\nIncorrect: {result.IncorrectCount}");
                    OutputText.FileOutput = "";
                }); ;



        }
    }
}
