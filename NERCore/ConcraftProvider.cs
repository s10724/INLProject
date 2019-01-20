using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NERCore
{
    public abstract class ConcraftProvider
    {
        private Process serverProcess;
        public bool IsServerWorking { get; set; }
        public void StartServer(string concraftPath, string concraftModelPath)
        {
                serverProcess = new Process();
                serverProcess.StartInfo.FileName = concraftPath;
                serverProcess.StartInfo.Arguments = $"server --port=3000 -i {concraftModelPath} +RTS -N4";
                serverProcess.StartInfo.UseShellExecute = false;
            serverProcess.StartInfo.CreateNoWindow = true;
                serverProcess.StartInfo.StandardOutputEncoding = Encoding.Default;
                serverProcess.StartInfo.StandardErrorEncoding = Encoding.Default;
                serverProcess.StartInfo.RedirectStandardOutput = true;
                serverProcess.StartInfo.RedirectStandardError = true;
                serverProcess.OutputDataReceived += Server_OutputDataReceived;
                serverProcess.ErrorDataReceived += Server_OutputDataReceived;
                serverProcess.Start();
                serverProcess.BeginOutputReadLine();
                serverProcess.BeginErrorReadLine();
        }

        public void StopServer()
        {
            serverProcess?.Kill();
            IsServerWorking = false;
        }

        private void Server_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
           if(!IsServerWorking && (e?.Data?.Contains("3000") ?? false))
            {
                IsServerWorking = true;
            }
        }

        StringBuilder builder = new StringBuilder();
        protected string Run(string text,string fileName = "")
        {
            OutputText.WriteLine($"Start tag: {text}");
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Anaconda3_64\python.exe";
            process.StartInfo.Arguments = $"D:\\NKJP\\INLProject\\PythonScripts\\ConcraftProvider.py \"{text}\"";
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
       
            OutputText.WriteLine($"End tag");
            string result = builder.ToString();
            if(!string.IsNullOrWhiteSpace(fileName))
                SaveTagResultToFile(fileName, result);
            return result;
        }

        protected void SaveTagResultToFile(string fileName, string result)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write($"{result}\n");
            }
        }


        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            builder.AppendLine(e.Data);
        }
    }
}
