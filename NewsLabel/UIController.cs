using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class UIController
    {
        private static readonly int BACK_LIMIT_DEFAULT = 2;

        public NewsPair CurrentNewsPair;
        public ProgressManager ProgressManager;

        private LabelWork labelWork;
        private NewsFileHandler handler;
        private List<NewsPair> currentActivePairs;
        private int currentNewsPairIndex = 0;
        private int backLimit = BACK_LIMIT_DEFAULT;
        private bool isAtEOF;

        public delegate void OnControllerInitDelegate();
        public delegate void OnNewsFileHandlerInitDelegate();
        public delegate void OnNewsFileHandlerDestroyDelegate();
        public delegate void OnNewsContentUpdateDelegate();
        public delegate void OnEndOfFileReachedDelegate(bool canGoToPrev);
        public delegate void OnNextNewsPairDelegate(bool canGoToPrev);
        public delegate void OnPreviousNewsPairDelegate(bool canGoToPrev);

        public OnNewsFileHandlerInitDelegate OnNewsFileHandlerInit;
        public OnNewsFileHandlerDestroyDelegate OnNewsFileHandlerDestroy;
        public OnEndOfFileReachedDelegate OnEndOfFileReached;
        public OnNextNewsPairDelegate OnNextNewsPair;
        public OnPreviousNewsPairDelegate OnPreviousNewsPair;
        public OnNewsContentUpdateDelegate OnNewsContentUpdate;

        public UIController()
        {
            currentActivePairs = new List<NewsPair>();
            ProgressManager = new ProgressManager();
        }

        public UIController(int backLimit)
        {
            currentActivePairs = new List<NewsPair>();
            ProgressManager = new ProgressManager();
            if (backLimit > BACK_LIMIT_DEFAULT)
            {
                this.backLimit = backLimit;
            }
            else
            {
                this.backLimit = BACK_LIMIT_DEFAULT;
            }
        }

        public string getCurrentLineNum()
        {
            if(CurrentNewsPair != null)
            {
                return CurrentNewsPair.LineNum.ToString();
            }
            else
            {
                return "0";
            }
        }

        public string getOutputFileName()
        {
            return handler.OutputFileName;
        }

        public void DestroyNewsFileHandler()
        {
            if(handler == null)
            {
                return;
            }
            int end = isAtEOF ? currentActivePairs.Count() : Math.Min(currentActivePairs.Count()-1, currentNewsPairIndex);
            for (int i = 0; i < end; i++)
            {
                handler.WriteToOutputFile(currentActivePairs.ElementAt(i));
            }
            handler.Destroy();
            currentActivePairs.Clear();
            currentNewsPairIndex = 0;
            handler = null;
            CurrentNewsPair = null;
            OnNewsFileHandlerDestroy?.Invoke();
        }

        public void InitNewsFileHandler(LabelWork work)
        {
            if(handler != null)
            {
                DestroyNewsFileHandler();
            }
            labelWork = work;
            handler = new NewsFileHandler(new NewsFileHandlerConfig(), onContentUpdate, work);
            OnNewsFileHandlerInit?.Invoke();
        }

        public void SetCurrentNewsPairRelation(string relation)
        {
            CurrentNewsPair.RelationLabel = relation;
        }

        public void LoadPreviousNewsPair()
        {
            if(currentNewsPairIndex > 0 && currentNewsPairIndex < currentActivePairs.Count())
            {
                CurrentNewsPair = currentActivePairs.ElementAt(--currentNewsPairIndex);
            }
            OnPreviousNewsPair?.Invoke(currentNewsPairIndex > 0);
        }

        public void InitActiveNewsPairList()
        {
            int i = 0;
            while (i++ < backLimit && !addNextNewsPairToList()) ;
            if(currentActivePairs.Count() > 0)
            {
                CurrentNewsPair = currentActivePairs.ElementAt(currentNewsPairIndex);
                OnNextNewsPair?.Invoke(currentNewsPairIndex > 0);
            }
            else
            {
                OnEndOfFileReached?.Invoke(currentActivePairs.Count() > 0);
            }
        }

        private void onContentUpdate(NewsPair p)
        {
            if (p == CurrentNewsPair)
            {
                OnNewsContentUpdate?.Invoke();
            }
        }

        public void LoadNextNewsPair()
        {
            isAtEOF = false;
            if(currentNewsPairIndex >= currentActivePairs.Count() - 1)
            {
                writeNewsPairToFile();
                isAtEOF = addNextNewsPairToList();
            }
            if (!isAtEOF)
            {
                currentNewsPairIndex++;
                CurrentNewsPair = currentActivePairs.ElementAt(currentNewsPairIndex);
                OnNextNewsPair?.Invoke(currentNewsPairIndex > 0);
            }
            else
            {
                OnEndOfFileReached?.Invoke(currentActivePairs.Count() > 0);
            }
        }

        private void writeNewsPairToFile()
        {
            if(currentActivePairs.Count() == backLimit)
            {
                NewsPair val = currentActivePairs.ElementAt(0);
                handler.WriteToOutputFile(val);
                currentNewsPairIndex--;
                currentActivePairs.RemoveAt(0);
            }
        }

        private bool addNextNewsPairToList()
        {
            NewsPair next = handler.ParseNextLine();
            if(next == null)
            {
                return true;
            }
            currentActivePairs.Add(next);
            return false;
        }

        public void WriteProgress()
        {
            if(labelWork == null || labelWork.ProgressFileName.Length == 0)
            {
                return;
            }
            if (isAtEOF)
            {
                ProgressManager.deleteSaveFile(labelWork);
            }
            else if (CurrentNewsPair != null)
            {
                ProgressManager.createNewSaveFile(CurrentNewsPair.LineNum, labelWork);
            }
        }
    }
}
