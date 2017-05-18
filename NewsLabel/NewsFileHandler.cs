using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class NewsFileHandler
    {
        public static readonly int ENCODING_CHINESE_SIMPLIFIED = 936;
        private NewsFileHandlerConfig config;
        private StreamReader inputFileReader;
        private StreamReader newsFileReader;
        private StreamWriter outputFileWriter;
        private StreamWriter uncertainOutputFileWriter;
        private BackgroundWorker contentReadWorker;
        private ConcurrentQueue<NewsPair> contentReadQ;
        private readonly string LOADING = "读取中";
        public string OutputFileName;
        public int CurrentLineNum = 0;
        public int CurrentContentFileLineNum = 0;

        public delegate void OnNewsContentUpdateDelegate(NewsPair p);
        public OnNewsContentUpdateDelegate OnNewsContentUpdate;
        private object sharedWorkerResourceLock = new object();

        private LabelWork labelWork;

        public NewsFileHandler(NewsFileHandlerConfig conf, OnNewsContentUpdateDelegate del, LabelWork work)
        {
            config = conf;
            labelWork = work;
            inputFileReader = new StreamReader(labelWork.SourceFilePath, Encoding.UTF8, true);
            contentReadQ = new ConcurrentQueue<NewsPair>();
            if (labelWork.ContentFilePath.Length != 0)
            {
                newsFileReader = new StreamReader(new FileStream(labelWork.ContentFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite), Encoding.UTF8, true);
            }
            OutputFileName = labelWork.OutputFileName + labelWork.OutputFileExtension;
            outputFileWriter = new StreamWriter(new FileStream(OutputFileName, FileMode.Append, FileAccess.Write), Encoding.UTF8);
            OnNewsContentUpdate = del;
            advanceReaderToLine(work.StartingLineNum);
        }

        private void advanceReaderToLine(int targetLineNum)
        {
            if(inputFileReader == null || targetLineNum <= 1)
            {
                return;
            }
            while(CurrentLineNum < targetLineNum - 1 && inputFileReader.ReadLine() != null)
            {
                CurrentLineNum++;
                newsFileReader?.ReadLine();
            }
        }

        private void startContentReadWorker(NewsPair pair)
        {
            if(pair == null)
            {
                return;
            }
            contentReadWorker = new BackgroundWorker();
            contentReadWorker.WorkerReportsProgress = false;
            contentReadWorker.WorkerSupportsCancellation = true;
            contentReadWorker.DoWork += worker_DoWork;
            contentReadWorker.RunWorkerCompleted += worker_RunWorkerCompleted;
            contentReadWorker.RunWorkerAsync(pair);
        }

        private void notifyContentReadWorker()
        {
            if ( (contentReadWorker == null || !contentReadWorker.IsBusy) && !contentReadQ.IsEmpty)
            {
                NewsPair next;
                contentReadQ.TryDequeue(out next);
                startContentReadWorker(next);
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            NewsPair pair = e.Argument as NewsPair;
            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
                return;
            }
            string[] rv = new string[config.NumElementNewsContentFile];
            for (int i = 0; i < rv.Length; i++)
            {
                rv[i] = "(无内容)";
            }
            lock (sharedWorkerResourceLock)
            {
                if (newsFileReader == null)
                {
                    setNewsContentForPair(pair, rv);
                    return;
                }
                string line = newsFileReader.ReadLine();
                if (line == null)
                {
                    setNewsContentForPair(pair, rv);
                    return;
                }
                CurrentContentFileLineNum++;
                string[] split = line.Split(config.SeparatorRead, StringSplitOptions.None);
                if (split.Length != config.NumElementNewsContentFile)
                {
                    setNewsContentForPair(pair, rv);
                    return;
                }
                setNewsContentForPair(pair, split); 
            }
            e.Result = pair;
        }

        private void setNewsContentForPair(NewsPair pair, string[] contentArray)
        {
            pair.News1Content = contentArray[config.News1ContentIndexInContentFile];
            pair.News2Content = contentArray[config.News2ContentIndexInContentFile];
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                return;
            }
            else if (!(e.Error == null))
            {
                return;
            }
            else
            {
                OnNewsContentUpdate?.Invoke((NewsPair)e.Result);
                if (!contentReadQ.IsEmpty)
                {
                    NewsPair next;
                    contentReadQ.TryDequeue(out next);
                    startContentReadWorker(next);
                }
            }
        }

        private void cleanOutputFile(string fullName)
        {
            if (File.Exists(fullName))
            {
                File.Delete(fullName);
            }
        }

        public NewsPair ParseNextLine()
        {
            string line = inputFileReader.ReadLine();
            if(line == null)
            {
                return null;
            }
            CurrentLineNum++;
            string[] split = line.Split(config.SeparatorRead, StringSplitOptions.None);
            NewsPair rv = null;
            try
            {
                if (split.Length >= config.NumElementFullSourceFile)
                {
                    rv = new NewsPair(CurrentLineNum, split, split[config.News1TitleIndex],
                    split[config.News1ContentIndex],
                    split[config.News2TitleIndex],
                    split[config.News2ContentIndex],
                    (int)double.Parse(split[config.CommonTitleWordsIndex])
                    );
                }
                else if(split.Length >= config.NumElementSimplifiedSourceFile)
                {
                    rv = new NewsPair(CurrentLineNum, split, split[config.News1TitleIndex],
                    LOADING,
                    split[config.News2TitleIndex],
                    LOADING,
                    (int)double.Parse(split[config.CommonTitleWordsIndex])
                    );
                    contentReadQ.Enqueue(rv);
                    notifyContentReadWorker();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return rv;
        }

        private string getOutputString(NewsPair pair)
        {
            string[] tmp = new string[pair.OriginalContent.Length];
            for(int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = pair.OriginalContent[i];
            }
            tmp[config.RelationLabelIndex] = pair.RelationLabel;
            return string.Join(config.SeparatorWrite, tmp);
        }

        private string getUncertainOutputString(NewsPair pair)
        {
            string[] tmp = new string[pair.OriginalContent.Length + 1];
            for (int i = 0; i < pair.OriginalContent.Length; i++)
            {
                tmp[i] = pair.OriginalContent[i];
            }
            tmp[config.RelationLabelIndex] = pair.RelationLabel;
            tmp[pair.OriginalContent.Length] = pair.LineNum.ToString();
            return string.Join(config.SeparatorWrite, tmp);
        }

        public void WriteToOutputFile(NewsPair pair)
        {
            string val = getOutputString(pair);
            outputFileWriter.WriteLine(val);
            outputFileWriter.Flush();
            if (pair.LabelUncertain)
            {
                WriteToUncertainOutputFile(pair);
            }
        }

        private void WriteToUncertainOutputFile(NewsPair pair)
        {
            if(uncertainOutputFileWriter == null)
            {
                uncertainOutputFileWriter = new StreamWriter(new FileStream(labelWork.UncertainOutputFileName, FileMode.Append, FileAccess.Write), Encoding.UTF8);
            }
            string val = getUncertainOutputString(pair);
            uncertainOutputFileWriter.WriteLine(val);
            uncertainOutputFileWriter.Flush();
        }

        public void Destroy()
        {
            inputFileReader?.Close();
            inputFileReader = null;
            outputFileWriter?.Close();
            outputFileWriter = null;
            uncertainOutputFileWriter?.Close();
            uncertainOutputFileWriter = null;
            lock (sharedWorkerResourceLock)
            {
                newsFileReader?.Close();
                newsFileReader = null;
            }
        }
    }
}
