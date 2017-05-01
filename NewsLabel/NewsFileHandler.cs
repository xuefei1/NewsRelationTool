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
        private BackgroundWorker contentReadWorker;
        private ConcurrentQueue<NewsPair> contentReadQ;
        private readonly string LOADING = "Loading";
        public string OutputFileName;
        public int CurrentLineNum = 0;
        public int CurrentContentFileLineNum = 0;

        public delegate void OnNewsContentUpdateDelegate(NewsPair p);
        public OnNewsContentUpdateDelegate OnNewsContentUpdate;
        private object sharedWorkerResourceLock = new object();

        public NewsFileHandler(NewsFileHandlerConfig conf, OnNewsContentUpdateDelegate del, string fullFileName, string newsFileName = "")
        {
            config = conf;
            inputFileReader= new StreamReader(fullFileName, Encoding.UTF8, true);
            contentReadQ = new ConcurrentQueue<NewsPair>();
            if (newsFileName.Length != 0)
            {
                newsFileReader = new StreamReader(newsFileName, Encoding.UTF8, true);
            }
            OutputFileName = Path.GetFileNameWithoutExtension(fullFileName) + "_output.csv";
            cleanOutputFile(OutputFileName);
            outputFileWriter = new StreamWriter(new FileStream(OutputFileName, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            OnNewsContentUpdate = del;
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
                rv[i] = "(no content found in news file)";
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
                if (split.Length == config.NumElementFullSourceFile)
                {
                    rv = new NewsPair(CurrentLineNum, split, split[config.News1TitleIndex],
                    split[config.News1ContentIndex],
                    split[config.News2TitleIndex],
                    split[config.News2ContentIndex],
                    (int)double.Parse(split[config.CommonTitleWordsIndex])
                    );
                }
                else if(split.Length == config.NumElementSimplifiedSourceFile)
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
            string[] tmp = new string[pair.OriginalContent.Length - 2];
            for(int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = pair.OriginalContent[i];
            }
            tmp[config.RelationLabelIndex] = pair.RelationLabel;
            return string.Join(config.SeparatorWrite, tmp);
        }

        public void WriteToOutputFile(NewsPair pair)
        {
            string val = getOutputString(pair);
            outputFileWriter.WriteLine(val);
            outputFileWriter.Flush();
        }

        public void Destroy()
        {
            inputFileReader?.Close();
            inputFileReader = null;
            outputFileWriter?.Close();
            outputFileWriter = null;
            lock (sharedWorkerResourceLock)
            {
                newsFileReader?.Close();
                newsFileReader = null;
            }
        }
    }
}
