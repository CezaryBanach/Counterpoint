using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint
{
    class TextFileLog
    {
        private string FilePath { get; set; }

        private string defaultPath = @"C:\Users\Czarek\source\repos\Counterpoint\Log\Log.txt";

        public TextFileLog()
        {
            FilePath = defaultPath;
        }
        public TextFileLog(string path)
        {
            FilePath = path;
        }

        public void WriteLog(string log)
        {
            if(File.Exists(FilePath))
            {
                using (StreamWriter file = File.AppendText(FilePath))
                {
                    file.WriteLine(log);
                }
            }
        }


    }
}
