using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class NewsFileHandlerConfig
    {
        private string[] separatorRead = { "|" };
        private string separatorWrite = "|";
        private int numElementFullSourceFile = 6;
        private int numElementSourceFile = 4;
        private int numElementNewsContentFile = 4;
        private int relationLabelIndex = 0;
        private int news1TitleIndex = 2;
        private int news2TitleIndex = 3;
        private int news1ContentIndex = 4;
        private int news2ContentIndex = 5;
        private int news1TitleIndexInContentFile = 0;
        private int news2TitleIndexInContentFile = 1;
        private int news1ContentIndexInContentFile = 2;
        private int news2ContentIndexInContentFile = 3;
        private int commonTitleWordsIndex = 1;

        public int NumElementFullSourceFile
        {
            get
            {
                return numElementFullSourceFile;
            }
        }
        public int NumElementSimplifiedSourceFile
        {
            get
            {
                return numElementSourceFile;
            }
        }
        public int NumElementNewsContentFile
        {
            get
            {
                return numElementNewsContentFile;
            }
        }
        public string[] SeparatorRead
        {
            get
            {
                return separatorRead;
            }
        }
        public string SeparatorWrite
        {
            get
            {
                return separatorWrite;
            }
        }
        public int RelationLabelIndex
        {
            get
            {
                return relationLabelIndex;
            }
        }
        public int CommonTitleWordsIndex
        {
            get
            {
                return commonTitleWordsIndex;
            }
        }
        public int News1TitleIndex
        {
            get
            {
                return news1TitleIndex;
            }
        }
        public int News1TitleIndexInContentFile
        {
            get
            {
                return news1TitleIndexInContentFile;
            }
        }
        public int News1ContentIndex
        {
            get
            {
                return news1ContentIndex;
            }
        }
        public int News1ContentIndexInContentFile
        {
            get
            {
                return news1ContentIndexInContentFile;
            }
        }
        public int News2TitleIndex
        {
            get
            {
                return news2TitleIndex;
            }
        }
        public int News2TitleIndexInContentFile
        {
            get
            {
                return news2TitleIndexInContentFile;
            }
        }
        public int News2ContentIndex
        {
            get
            {
                return news2ContentIndex;
            }
        }
        public int News2ContentIndexInContentFile
        {
            get
            {
                return news2ContentIndexInContentFile;
            }
        }
    }
}
