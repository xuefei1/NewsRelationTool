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
        private readonly string NO_SRC_FILE_SELECTED = "(No source file selected)";
        private readonly string SRC_FILE_HEADER = "Source file:    ";
        private readonly string NEWS_CONTENT_FILE_HEADER = "Optional news content file:    ";
        private readonly string OUTPUT_FILE_HEADER = "Output file:    ";
        private readonly string COMMON_TITLE_WORDS_HEADER = "Common words in title:    ";
        private readonly string LINE_NUM_HEADER = "Current line number in source file:    ";
        private string sourceFilePath = "";
        private string newsFilePath = "";
        public MainWindow()
        {
            InitializeComponent();
            controller = new UIController();
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

        private void onControllerInit()
        {
            button_subsumption.Content += " (" + config.Subsumption + ")";
            button_notrelated.Content += " (" + config.NotRelated + ")";
            button_related.Content += " (" + config.Related + ")";
            button_overlap.Content += " (" + config.Overlap + ")";
            button_followup.Content += " (" + config.FollowUp + ")";
            button_contradict.Content += " (" + config.Contradict + ")";
            button_equivalent.Content += " (" + config.Equivalent + ")";
            button_start.IsEnabled = false;
            onNewsFileHandlerDestroy();
        }

        private void onNewsFileHandlerInit()
        {
            button_start.IsEnabled = false;
            button_selectnewsfile.IsEnabled = false;
            button_previous.IsEnabled = false;
            button_selectfile.IsEnabled = false;
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
        }

        private void onNewsFileHandlerDestroy()
        {
            button_done.IsEnabled = false;
            button_selectfile.IsEnabled = true;
            button_previous.IsEnabled = false;
            button_selectnewsfile.IsEnabled = true;
            disableControlButtons();
            setDefaultNewsContent();
            textblock_sourcefilepath.Text = SRC_FILE_HEADER;
            textblock_newsfilepath.Text = NEWS_CONTENT_FILE_HEADER;
            textblock_outputfilepath.Text = OUTPUT_FILE_HEADER;
            textblock_commontitlewords.Text = COMMON_TITLE_WORDS_HEADER;
            textblock_linenum.Text = LINE_NUM_HEADER;
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
        }

        private void disableControlButtons()
        {
            button_done.IsEnabled = false;
            button_notrelated.IsEnabled = false;
            button_related.IsEnabled = false;
            button_followup.IsEnabled = false;
            button_contradict.IsEnabled = false;
            button_overlap.IsEnabled = false;
            button_subsumption.IsEnabled = false;
            button_equivalent.IsEnabled = false;
        }

        private void enableControlButtons()
        {
            button_done.IsEnabled = true;
            button_notrelated.IsEnabled = true;
            button_related.IsEnabled = true;
            button_followup.IsEnabled = true;
            button_contradict.IsEnabled = true;
            button_overlap.IsEnabled = true;
            button_subsumption.IsEnabled = true;
            button_equivalent.IsEnabled = true;
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

        private void onEndOfFileReached(bool canGoToPrev)
        {
            disableControlButtons();
            button_done.IsEnabled = true;
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
            enableControlButtons();
            updateNewsPairText();
            if (canGoToPrev)
            {
                button_previous.IsEnabled = true;
            }
            else
            {
                button_previous.IsEnabled = false;
            }
        }

        private void onNextPairLoad(bool canGoToPrev)
        {
            dockpanel_eof_mask.Visibility = Visibility.Collapsed;
            enableControlButtons();
            updateNewsPairText();
            if (canGoToPrev)
            {
                button_previous.IsEnabled = true;
            }
            else
            {
                button_previous.IsEnabled = false;
            }
        }

        private void updatePathText()
        {
            textblock_sourcefilepath.Text = SRC_FILE_HEADER + sourceFilePath;
            textblock_newsfilepath.Text = NEWS_CONTENT_FILE_HEADER + newsFilePath;
        }

        private void button_selectfile_click(object sender, RoutedEventArgs e)
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
                button_start.IsEnabled = true;
                updatePathText();
            }
        }

        private void button_done_click(object sender, RoutedEventArgs e)
        {
            controller.DestroyNewsFileHandler();
        }

        private void tmpDisableOnClick()
        {
            disableControlButtons();
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

        private void button_selectnewsfile_click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "All files (*.*)|*.*|CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                newsFilePath = dlg.FileName;
                updatePathText();
            }
        }

        private void button_start_click(object sender, RoutedEventArgs e)
        {
            controller.InitNewsFileHandler(sourceFilePath, newsFilePath);
            refresh();
            enableControlButtons();
            controller.InitActiveNewsPairList();
            textblock_outputfilepath.Text = OUTPUT_FILE_HEADER + controller.getOutputFileName();
            textblock_linenum.Text = LINE_NUM_HEADER + controller.getCurrentLineNum();
            textblock_commontitlewords.Text = COMMON_TITLE_WORDS_HEADER + controller.CurrentNewsPair?.CommonTitleWords.ToString();
        }

        private static Action EmptyDelegate = delegate () { };
        private void refresh()
        {
            Dispatcher.Invoke(DispatcherPriority.SystemIdle, EmptyDelegate);
        }
    }
}
