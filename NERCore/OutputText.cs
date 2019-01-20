using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NERCore
{
    public static class OutputText
    {
        public static string FileOutput { get; set; }

        public static void WriteLine(string text)
        {
            Console.WriteLine(text);
            SaveToFile(text,true);
        }

        public static void Write(string text)
        {
            Console.Write(text);
            SaveToFile(text);
        }
        private static object locker = new Object();
        private static void SaveToFile(string text, bool addNewLine = false)
        {
            if(!string.IsNullOrWhiteSpace(FileOutput))
            {
                lock (locker)
                {
                    using (StreamWriter writer = new StreamWriter(FileOutput, true))
                    {
                        if (addNewLine)
                        {
                            writer.WriteLine(text);
                        }
                        else
                        {
                            writer.Write(text);
                        }
                    }
                }
            }
        }
    }
}
