using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class NewsPair : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private bool contentReadComplete = false;
        public bool ContentReadComplete
        {
            get
            {
                return contentReadComplete;
            }
            set
            {
                contentReadComplete = value;
                OnPropertyChanged();
            }
        }
        private int lineNum;
        public int LineNum
        {
            get
            {
                return lineNum;
            }
            set
            {
                lineNum = value;
                OnPropertyChanged();
            }
        }

        private string[] content;
        public string[] OriginalContent
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }

        private string relationLabel = "N";
        public string RelationLabel
        {
            get
            {
                return relationLabel;
            }
            set
            {
                relationLabel = value;
                OnPropertyChanged();
            }
        }


        private int commonTitleWords = 0;
        public int CommonTitleWords
        {
            get
            {
                return commonTitleWords;
            }
            set
            {
                commonTitleWords = value;
                OnPropertyChanged();
            }
        }

        private string news1Title = "(No news article found)";
        public string News1Title
        {
            get
            {
                return news1Title;
            }
            set
            {
                news1Title = value;
                OnPropertyChanged();
            }
        }

        private string news1Content = "(No news article found)";
        public string News1Content
        {
            get
            {
                return news1Content;
            }
            set
            {
                news1Content = value;
                OnPropertyChanged();
            }
        }

        private string news2Title = "(No news article found)";
        public string News2Title
        {
            get
            {
                return news2Title;
            }
            set
            {
                news2Title = value;
                OnPropertyChanged();
            }
        }

        private string news2Content = "(No news article found)";
        public string News2Content
        {
            get
            {
                return news2Content;
            }
            set
            {
                news2Content = value;
                OnPropertyChanged();
            }
        }

        public NewsPair(int lineNum, 
            string[] originalContent, 
            string title1, 
            string content1, 
            string title2, 
            string content2, 
            int commonTitleWords = 0)
        {
            LineNum = lineNum;
            OriginalContent = originalContent;
            News1Title = title1;
            News1Content = content1;
            News2Title = title2;
            News2Content = content2;
            CommonTitleWords = commonTitleWords;
        }
    }
}
