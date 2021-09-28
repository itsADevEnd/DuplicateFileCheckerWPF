using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileCheckerWPF
{
    internal class Settings
    {
        public string LogDirectory { get; set; }

        public Settings(string logDirectory)
        {
            LogDirectory = logDirectory;
        }
    }
}
