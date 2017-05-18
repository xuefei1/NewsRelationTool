using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NewsLabel
{
    public class ChineseDateTimeMatcher
    {
        private string patternMonthDay = @"[\s]*[0-9]+[\s]*月[\s]*[0-9]+[\s]*[日|号]";

        public void HighlightMonthDayInString(TextBlock block, string text)
        {
            Run run;
            Match m = Regex.Match(text, patternMonthDay);
            while (m.Success)
            {
                run = new Run { Text = text.Substring(0, m.Index) };
                block.Inlines.Add(run);
                run = new Run { Text = text.Substring(m.Index, m.Length), Background = Brushes.Orange };
                block.Inlines.Add(run);
                text = text.Substring(m.Index + m.Length, text.Length - (m.Index + m.Length));
                m = Regex.Match(text, patternMonthDay);
            }
            run = new Run { Text = text };
            block.Inlines.Add(run);
        }
    }
}
