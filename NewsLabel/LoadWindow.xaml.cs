using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        private static readonly string SOURCE_FILE_NOT_FOUND_MSG = "无法读取：找不到源文件";
        private static readonly string UNKNOWN_ERROR_MSG = "无法读取, {0}";

        private ProgressManager progressManager;
        private LabelWork currWork;
        public LabelWork Result;

        public LoadWindow(ProgressManager p)
        {
            progressManager = p;
            InitializeComponent();
            reloadSaves();
            listbox_saves.SelectionChanged += listbox_saves_SelectionChanged;
            textbox_outputfile.TextChanged += Textbox_outputfile_TextChanged;
            textbox_uncertainfile.TextChanged += Textbox_uncertainfile_TextChanged;
        }

        private void reloadSaves()
        {
            progressManager.cleanCorruptedSaveFiles();
            listbox_saves.ItemsSource = progressManager.loadAllSaved();
            listbox_saves.Items.Refresh();
        }

        private void resetOnFailure()
        {
            reloadSaves();
            currWork = null;
            clearFileNames();
            checkReadyConditions();
        }

        private void Textbox_uncertainfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkReadyConditions();
        }

        private void Textbox_outputfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkReadyConditions();
        }

        private void displayMsgDialog(string msg)
        {
            MessageBoxButton btnMessageBox = MessageBoxButton.OK;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
            MessageBoxResult rsltMessageBox = MessageBox.Show(msg, "", btnMessageBox, icnMessageBox);
        }

        private void button_load_click(object sender, RoutedEventArgs e)
        {
            try
            {
                currWork = progressManager.loadSavedLabelWork(currWork.ProgressFileName);
            }
            catch(SourceFileNotFoundException)
            {
                displayMsgDialog(SOURCE_FILE_NOT_FOUND_MSG);
                resetOnFailure();
                return;
            }
            catch(Exception e1)
            {
                displayMsgDialog(string.Format(UNKNOWN_ERROR_MSG, e1.StackTrace.ToString()));
                resetOnFailure();
                return;
            }
            Result = currWork;
            Close();
        }

        private bool checkReadyConditions()
        {
            if (currWork != null && textbox_outputfile.Text.Length > 0 && textbox_uncertainfile.Text.Length > 0)
            {
                readyToStart();
                return true;
            }
            else
            {
                notReadyToStart();
                return false;
            }
        }

        private void readyToStart()
        {
            button_load.IsEnabled = true;
        }

        private void notReadyToStart()
        {
            button_load.IsEnabled = false;
        }

        private void autoFillFileNames()
        {
            textbox_outputfile.Text = currWork?.OutputFileName;
            textbox_uncertainfile.Text = currWork?.UncertainOutputFileName;
        }

        private void clearFileNames()
        {
            textbox_outputfile.Text = "";
            textbox_uncertainfile.Text = "";
        }

        private void button_cancel_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void listbox_saves_SelectionChanged(object sender, RoutedEventArgs e)
        {
            currWork = listbox_saves.SelectedItem as LabelWork;
            clearFileNames();
            autoFillFileNames();
            checkReadyConditions();
        }
    }
}
