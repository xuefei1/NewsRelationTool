using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class LabelWork
    {
        public string SourceFilePath { get; set; }
        public string ContentFilePath { get; set; }
        public string OutputFileName { get; set; }
        public string OutputFileExtension { get; set; }
        public string UncertainOutputFileName { get; set; }
        public string UncertainOutputFileExtension { get; set; }
        public string ProgressFileName { get; set; }
        public int StartingLineNum { get; set; }

        public LabelWork() { }

        public LabelWork(string sourceFilePath, int startingLineNum, string outputFileName, string uncertainOutputFileName,
            string contentFilePath = "",  string outputFileExtension = "", 
            string uncertainOutputFileExtension = "", string progressFileName = "")
        {
            SourceFilePath = sourceFilePath;
            ContentFilePath = contentFilePath;
            OutputFileName = outputFileName;
            OutputFileExtension = outputFileExtension;
            ProgressFileName = progressFileName;
            StartingLineNum = startingLineNum;
            UncertainOutputFileName = uncertainOutputFileName;
            UncertainOutputFileExtension = uncertainOutputFileExtension;
        }
    }
}
