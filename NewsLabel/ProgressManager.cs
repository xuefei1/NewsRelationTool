using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace NewsLabel
{
    public class ProgressManager
    {
        public static readonly string PROGRESS_DIRECTORY_NAME = "_saves";

        public ProgressManager()
        {
            checkSaveDirectory();
        }

        private void checkSaveDirectory()
        {
            if (!Directory.Exists(PROGRESS_DIRECTORY_NAME))
            {
                Directory.CreateDirectory(PROGRESS_DIRECTORY_NAME);
            }
        }

        public void deleteSaveFile(LabelWork work)
        {
            string filePath = Path.Combine(getSaveDirectory(), work.ProgressFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void createNewSaveFile(int currLineNum, LabelWork work)
        {
            StreamWriter writer = new StreamWriter(new FileStream(Path.Combine(PROGRESS_DIRECTORY_NAME, work.ProgressFileName), FileMode.Create, FileAccess.Write), Encoding.UTF8);
            writer.WriteLine(work.SourceFilePath);
            writer.WriteLine(work.ContentFilePath);
            writer.WriteLine(currLineNum);
            writer.WriteLine(work.OutputFileName);
            writer.WriteLine(work.UncertainOutputFileName);
            writer.WriteLine(work.OutputFileExtension);
            writer.WriteLine(work.UncertainOutputFileExtension);
            writer.Flush();
            writer.Close();
        }

        private string getSaveDirectory()
        {
            string rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string directory = Path.Combine(rootDirectory, PROGRESS_DIRECTORY_NAME);
            return directory;
        }

        public LabelWork loadSavedLabelWork(string name)
        {
            LabelWork rv = null;
            checkSaveDirectory();
            string filePath = Path.Combine(getSaveDirectory(), name);
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true);
                string sourceFilePath = reader.ReadLine();
                if (!File.Exists(sourceFilePath))
                {
                    reader.Close();
                    throw new SourceFileNotFoundException();
                }
                string contentFilePath = reader.ReadLine();
                string startingLineNum = reader.ReadLine();
                string outputFileName = reader.ReadLine();
                string uncertainOutputFileName = reader.ReadLine();
                string outputFileExt = reader.ReadLine();
                string uncertainOutputFileExt = reader.ReadLine();
                rv = new LabelWork(sourceFilePath, int.Parse(startingLineNum), outputFileName, uncertainOutputFileName, 
                    contentFilePath, outputFileExt, uncertainOutputFileExt, name);
                reader.Close();
            }else
            {
                throw new IOException();
            }
            return rv;
        }

        public void cleanCorruptedSaveFiles()
        {
            List<string> corrupted = new List<string>();
            loadAllSaved(corrupted);
            foreach (string file in corrupted)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public List<LabelWork> loadAllSaved(List<string> corruptedSaveFiles = null)
        {
            List<LabelWork> rv = new List<LabelWork>();
            checkSaveDirectory();
            string[] files = Directory.GetFiles(getSaveDirectory());
            foreach(string name in files)
            {
                try
                {
                    LabelWork work = loadSavedLabelWork(Path.GetFileNameWithoutExtension(name));
                    if(work != null)
                    {
                        rv.Add(work);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    corruptedSaveFiles?.Add(name);
                } 
            }
            return rv;
        }
    }

    public class SourceFileNotFoundException : Exception
    {

    }
}
