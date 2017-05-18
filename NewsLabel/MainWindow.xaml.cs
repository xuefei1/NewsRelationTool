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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NewsLabel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UIController controller;
        NewsRelationConfig config;
        ChineseDateTimeMatcher matcher;
        StreamWriter lockFileWriter;
        private static readonly string NO_SRC_FILE_SELECTED = "(选择源文件来开始)";
        private static readonly string SRC_FILE_HEADER = "源文件: ";
        private static readonly string NEWS_CONTENT_FILE_HEADER = "内容文件: ";
        private static readonly string OUTPUT_FILE_HEADER = "输出文件: ";
        private static readonly string COMMON_TITLE_WORDS_HEADER = "标题中相同的词语数: ";
        private static readonly string LINE_NUM_HEADER = "当前源文件行: ";
        private static readonly string MASK_CONTENT_EOF = "没了";
        private static readonly string MASK_CONTENT_LOADING = "读取中...";
        private static readonly string DUPLICATE_INSTANCE = "程序已在当前文件夹中运行";
        private static readonly string LOCK_FILE_FOLDER_NAME = "_tmp";
        private static readonly string LOCK_FILE_NAME = "_lock";
        private bool lockSuccess = false;

        public MainWindow()
        {
            InitializeComponent();
            if (!lockCurrDirectory())
            {
                displayMsgDialog(DUPLICATE_INSTANCE);
                Close();
                return;
            }
            lockSuccess = true;
            controller = new UIController(3);
            config = new NewsRelationConfig();
            matcher = new ChineseDateTimeMatcher();
            controller.OnEndOfFileReached = onEndOfFileReached;
            controller.OnNewsFileHandlerDestroy = onNewsFileHandlerDestroy;
            controller.OnNewsFileHandlerInit = onNewsFileHandlerInit;
            controller.OnNextNewsPair = onNextPairLoad;
            controller.OnPreviousNewsPair = onPreviousPairLoad;
            controller.OnNewsContentUpdate = updateNewsPairText;
            onControllerInit();
        }

        private bool lockCurrDirectory()
        {
            if (!Directory.Exists(LOCK_FILE_FOLDER_NAME))
            {
                Directory.CreateDirectory(LOCK_FILE_FOLDER_NAME);
                return createLockFile();
            }else
            {
                return false;
            }
        }

        private void unlockCurrDirectory()
        {
            lockFileWriter?.Close();
            if (Directory.Exists(LOCK_FILE_FOLDER_NAME))
            {
                Directory.Delete(LOCK_FILE_FOLDER_NAME, true);
            }
        }

        private bool createLockFile()
        {
            string file = System.IO.Path.Combine(getLockDirectory(), LOCK_FILE_NAME);
            if (!File.Exists(file))
            {
                lockFileWriter = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.ReadWrite));
                return true;
            }else
            {
                return false;
            }
        }

        private string getLockDirectory()
        {
            string rootDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string directory = System.IO.Path.Combine(rootDirectory, LOCK_FILE_FOLDER_NAME);
            return directory;
        }

        private void displayMsgDialog(string msg)
        {
            MessageBoxButton btnMessageBox = MessageBoxButton.OK;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
            MessageBoxResult rsltMessageBox = MessageBox.Show(msg, "", btnMessageBox, icnMessageBox);
        }

        private void onControllerInit()
        {
            button_subsumption.Content += " (" + config.Subsumption + ")";
            button_notrelated.Content += " (" + config.NotRelated + ")";
            button_related.Content += " (" + config.Related + ")";
            button_overlap.Content += " (" + config.Overlap + ")";
            button_followup.Content += " (" + config.FollowUp + ")";
            button_contradict.Content += " (" + config.Contradict + ")";
            button_equivalent.Content += " (" + config.Equivalent + ")";
            onNewsFileHandlerDestroy();
        }

        private void onNewsFileHandlerInit()
        {
            button_previous.IsEnabled = false;
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
        }

        private void onNewsFileHandlerDestroy()
        {
            button_previous.IsEnabled = false;
            disableLabelControlButtons();
            setDefaultNewsContent();
            textblock_sourcefilepath.Text = SRC_FILE_HEADER;
            textblock_newsfilepath.Text = NEWS_CONTENT_FILE_HEADER;
            textblock_outputfilepath.Text = OUTPUT_FILE_HEADER;
            textblock_commontitlewords.Text = COMMON_TITLE_WORDS_HEADER;
            textblock_linenum.Text = LINE_NUM_HEADER;
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
        }

        private void disableFileControlButtons()
        {
            button_new_label.IsEnabled = false;
            button_load.IsEnabled = false;
        }

        private void enableFileControlButtons()
        {
            button_new_label.IsEnabled = true;
            button_load.IsEnabled = true;
        }

        private void disableLabelControlButtons()
        {
            button_notrelated.IsEnabled = false;
            button_related.IsEnabled = false;
            button_followup.IsEnabled = false;
            button_contradict.IsEnabled = false;
            button_overlap.IsEnabled = false;
            button_subsumption.IsEnabled = false;
            button_equivalent.IsEnabled = false;
            button_toggle_uncertain.IsEnabled = false;
        }

        private void enableLabelControlButtons()
        {
            button_notrelated.IsEnabled = true;
            button_related.IsEnabled = true;
            button_followup.IsEnabled = true;
            button_contradict.IsEnabled = true;
            button_overlap.IsEnabled = true;
            button_subsumption.IsEnabled = true;
            button_equivalent.IsEnabled = true;
            button_toggle_uncertain.IsEnabled = true;
        }

        private void setDefaultNewsContent()
        {
            textblock_news_1_title.Text = NO_SRC_FILE_SELECTED;
            textblock_news_1_content.Text = NO_SRC_FILE_SELECTED;
            textblock_news_2_title.Text = NO_SRC_FILE_SELECTED;
            textblock_news_2_content.Text = NO_SRC_FILE_SELECTED;
        }

        private void updateNewsPairText()
        {
            if(controller.CurrentNewsPair == null)
            {
                return;
            }
            textblock_news_1_title.Text = controller.CurrentNewsPair.News1Title;
            textblock_news_1_content.Inlines.Clear();
            textblock_news_2_content.Inlines.Clear();
            //textblock_news_1_content.Text = controller.CurrentNewsPair.News1Content;
            //textblock_news_2_content.Text = controller.CurrentNewsPair.News2Content;
            matcher.HighlightMonthDayInString(textblock_news_1_content, controller.CurrentNewsPair.News1Content);
            textblock_news_2_title.Text = controller.CurrentNewsPair.News2Title;
            matcher.HighlightMonthDayInString(textblock_news_2_content, controller.CurrentNewsPair.News2Content);
            textblock_linenum.Text = LINE_NUM_HEADER + controller.getCurrentLineNum();
            textblock_commontitlewords.Text = COMMON_TITLE_WORDS_HEADER + controller.CurrentNewsPair.CommonTitleWords.ToString();
        }

        private void displayLoadingScreen()
        {
            textblock_mask_content.Text = MASK_CONTENT_LOADING;
            dockpanel_eof_mask.Visibility = Visibility.Visible;
        }

        private void onEndOfFileReached(bool canGoToPrev)
        {
            disableLabelControlButtons();
            textblock_mask_content.Text = MASK_CONTENT_EOF;
            dockpanel_eof_mask.Visibility = Visibility.Visible;
            setDefaultNewsContent();
            if (canGoToPrev)
            {
                button_previous.IsEnabled = true;
            }
            else
            {
                button_previous.IsEnabled = false;
            }
        }

        private void onPreviousPairLoad(bool canGoToPrev)
        {
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
            controller.WriteProgress();
            enableLabelControlButtons();
            updateNewsPairText();
            if (canGoToPrev)
            {
                button_previous.IsEnabled = true;
            }
            else
            {
                button_previous.IsEnabled = false;
            }
            updateToggleButton();
        }

        private void onNextPairLoad(bool canGoToPrev)
        {
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
            controller.WriteProgress();
            enableLabelControlButtons();
            updateNewsPairText();
            if (canGoToPrev)
            {
                button_previous.IsEnabled = true;
            }
            else
            {
                button_previous.IsEnabled = false;
            }
            updateToggleButton();
        }

        private void updateToggleButton()
        {
            if(controller.CurrentNewsPair == null)
            {
                return;
            }
            if (controller.CurrentNewsPair.LabelUncertain)
            {
                button_toggle_uncertain.IsChecked = true;
            }else
            {
                button_toggle_uncertain.IsChecked = false;
            }
        }

        private void tmpDisableOnClick()
        {
            disableLabelControlButtons();
            button_previous.IsEnabled = false;
            refresh();
        }

        private void button_previous_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.LoadPreviousNewsPair();
        }

        private void button_overlap_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.Overlap);
            controller.LoadNextNewsPair();
        }

        private void button_related_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.Related);
            controller.LoadNextNewsPair();
        }

        private void button_contrdict_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.Contradict);
            controller.LoadNextNewsPair();
        }

        private void button_followup_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.FollowUp);
            controller.LoadNextNewsPair();
        }

        private void button_notrelated_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.NotRelated);
            controller.LoadNextNewsPair();
        }

        private void button_notsure_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.NotSure);
            controller.LoadNextNewsPair();
        }

        private void button_subsumption_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.Subsumption);
            controller.LoadNextNewsPair();
        }

        private void button_equivalent_click(object sender, RoutedEventArgs e)
        {
            tmpDisableOnClick();
            controller.SetCurrentNewsPairRelation(config.Equivalent);
            controller.LoadNextNewsPair();
        }

        private static Action EmptyDelegate = delegate () { };
        private void refresh()
        {
            Dispatcher.Invoke(DispatcherPriority.SystemIdle, EmptyDelegate);
        }

        private void startNewLabelWork(LabelWork work)
        {
            disableFileControlButtons();
            controller.InitNewsFileHandler(work);
            refresh();
            enableLabelControlButtons();
            controller.InitActiveNewsPairList();
            textblock_sourcefilepath.Text = SRC_FILE_HEADER + work.SourceFilePath;
            textblock_newsfilepath.Text = NEWS_CONTENT_FILE_HEADER + work.ContentFilePath;
            textblock_outputfilepath.Text = OUTPUT_FILE_HEADER + controller.getOutputFileName();
            textblock_linenum.Text = LINE_NUM_HEADER + controller.getCurrentLineNum();
            textblock_commontitlewords.Text = COMMON_TITLE_WORDS_HEADER + controller.CurrentNewsPair?.CommonTitleWords.ToString();
            enableFileControlButtons();
        }

        private void button_new_label_click(object sender, RoutedEventArgs e)
        {
            NewLabelWindow newLabel = new NewLabelWindow();
            newLabel.ShowDialog();
            LabelWork result = newLabel.Result;
            if (result != null)
            {
                controller.WriteProgress();
                startNewLabelWork(result);
            }
        }

        private void button_load_click(object sender, RoutedEventArgs e)
        {
            LoadWindow win = new LoadWindow(controller.ProgressManager);
            win.ShowDialog();
            LabelWork result = win.Result;
            if(result != null)
            {
                controller.WriteProgress();
                if(result.StartingLineNum > 100)
                {
                    displayLoadingScreen();
                }
                startNewLabelWork(result);
            }
        }

        private void button_toggle_uncertain_Unchecked(object sender, RoutedEventArgs e)
        {
            if(controller.CurrentNewsPair != null)
                controller.CurrentNewsPair.LabelUncertain = false;
        }

        private void button_toggle_uncertain_Checked(object sender, RoutedEventArgs e)
        {
            if (controller.CurrentNewsPair != null)
                controller.CurrentNewsPair.LabelUncertain = true;
        }

        private void OnMainWindowClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller?.DestroyNewsFileHandler();
            controller?.WriteProgress();
            if(lockSuccess)
                unlockCurrDirectory();
        }
    }
}
