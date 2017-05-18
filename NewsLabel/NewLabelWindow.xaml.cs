using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NewsLabel
{
    /// <summary>
    /// Interaction logic for NewLabelWindow.xaml
    /// </summary>
    public partial class NewLabelWindow : Window
    {

        public LabelWork Result;

        private static readonly string OUTPUT_FILE_EXISTS_MSG = "输出文件 {0} 已存在， 新的内容会在原有内容后添加， 继续？";

        private static readonly string PROGRESS_FILE_EXISTS_MSG = "进度文件 {0} 已存在， 使用这个文件名会覆盖之前进度， 继续？";

        private static readonly string EXTENSION_NONE = "";

        private string sourceFilePath = "";
        private string contentFilePath = "";

        public NewLabelWindow()
        {
            InitializeComponent();
            textbox_outputfile.TextChanged += Textbox_outputfile_TextChanged;
            textbox_uncertainfile.TextChanged += Textbox_uncertainfile_TextChanged;
        }

        private void clearFileNames()
        {
            textbox_outputfile.Text = "";
            textbox_uncertainfile.Text = "";
            textbox_savefile.Text = "";
        }

        private void updateFilePath()
        {
            textblock_sourcefilepath.Text = sourceFilePath;
            textblock_newsfilepath.Text = contentFilePath;
        }

        private void Textbox_uncertainfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkStartCondition();
        }

        private void Textbox_outputfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkStartCondition();
        }

        private bool checkStartCondition()
        {
            if(sourceFilePath.Length > 0 && textbox_outputfile.Text.ToString().Length > 0 && 
                textbox_uncertainfile.Text.ToString().Length > 0)
            {
                readyToStart();
                return true;
            }else
            {
                notReadyToStart();
                return false;
            }
        }

        private void notReadyToStart()
        {
            button_start.IsEnabled = false;
        }

        private void readyToStart()
        {
            button_start.IsEnabled = true;
        }

        private bool progressFileExists()
        {
            return true;
        }

        private bool checkFileExists(string filename, string subfolder = "")
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
                subfolder);
            string filePath = System.IO.Path.Combine(directory, filename);
            return File.Exists(filePath);
        }

        private bool displayYesNoDialog(string msg)
        {
            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
            MessageBoxResult rsltMessageBox = MessageBox.Show(msg, "", btnMessageBox, icnMessageBox);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;
            }
            return false;
        }

        private void button_start_click(object sender, RoutedEventArgs e)
        {
            if (!checkStartCondition())
            {
                return;
            }
            if (!checkFileExists(sourceFilePath))
            {
                sourceFilePath = "";
                contentFilePath = "";
                clearFileNames();
                updateFilePath();
                checkStartCondition();
                return;
            }
            if(contentFilePath.Length > 0 && !checkFileExists(contentFilePath))
            {
                contentFilePath = "";
                updateFilePath();
            }
            string outputFileName = textbox_outputfile.Text.ToString();
            if (checkFileExists(outputFileName))
            {
                if (!displayYesNoDialog(string.Format(OUTPUT_FILE_EXISTS_MSG, outputFileName)))
                {
                    return;
                }
            }
            string uncertainOutputFileName = textbox_uncertainfile.Text.ToString();
            if (checkFileExists(uncertainOutputFileName))
            {
                if (!displayYesNoDialog(string.Format(OUTPUT_FILE_EXISTS_MSG, uncertainOutputFileName)))
                {
                    return;
                }
            }
            string progressFileName = textbox_savefile.Text.ToString();
            if (progressFileName.Length > 0 && checkFileExists(progressFileName, ProgressManager.PROGRESS_DIRECTORY_NAME))
            {
                if (!displayYesNoDialog(string.Format(PROGRESS_FILE_EXISTS_MSG, progressFileName)))
                {
                    return;
                }
            }
            Result = new LabelWork(sourceFilePath, 1, outputFileName, uncertainOutputFileName, contentFilePath, EXTENSION_NONE, EXTENSION_NONE, progressFileName);
            Close();
        }

        private void button_cancel_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void autoFillFileNames()
        {
            string sourceFile = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            if(textbox_outputfile.Text.Length == 0)
            {
                textbox_outputfile.Text = sourceFile + "_output";
            }
            if (textbox_uncertainfile.Text.Length == 0)
            {
                textbox_uncertainfile.Text = sourceFile + "_uncertain_output";
            }
            if (textbox_savefile.Text.Length == 0)
            {
                textbox_savefile.Text = sourceFile + "_save";
            }
        }

        private void button_select_source_click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "All files (*.*)|*.*|CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                sourceFilePath = dlg.FileName;
                clearFileNames();
                autoFillFileNames();
                checkStartCondition();
                textblock_sourcefilepath.Text = sourceFilePath;
            }
        }

        private void button_select_content_click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "All files (*.*)|*.*|CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                contentFilePath = dlg.FileName;
                textblock_newsfilepath.Text = contentFilePath;
            }
        }
    }
}
