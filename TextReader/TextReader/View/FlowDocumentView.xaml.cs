using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TextReader.ViewModel;

namespace TextReader.View
{
    /// <summary>
    /// Interaction logic for FlowDocumentView.xaml
    /// </summary>
    public partial class FlowDocumentView : UserControl
    {
        public FlowDocumentView()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(FlowDocumentView_DataContextChanged);
        }

        void FlowDocumentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fdvm = (DataContext as FlowDocumentViewModel);
            rtb.Document = fdvm.Document;
            fdvm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(fdvm_PropertyChanged);

            rtb.Selection.Select(fdvm.Selection.Start, fdvm.Selection.End);

            fdvm.ReadWord.Changed += new EventHandler(ReadWord_Changed);
            rtb.Selection.Changed += new EventHandler(Selection_Changed);
        }

        void fdvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Reading":
                    var fdvm = (DataContext as FlowDocumentViewModel);

                    if (!fdvm.Reading)
                    {
                        unindicate(true);
                        lastWord = null;
                        lastWordBackground = null;
                        lastParagraph = null;
                        lastParaBackground = null;
                    }
                    break;
                default:
                    break;
            }
        }

        void Selection_Changed(object sender, EventArgs e)
        {
            var fdvm = (DataContext as FlowDocumentViewModel);
            fdvm.Selection.Select(rtb.Selection.Start, rtb.Selection.End);
        }

        TextRange lastWord;
        object lastWordBackground;
        TextRange lastParagraph;
        object lastParaBackground;
        //Paragraph lastParagraph;
        void ReadWord_Changed(object sender, EventArgs e)
        {
            var fdvm = (DataContext as FlowDocumentViewModel);

            // Only do anything if it's being read
            if (!fdvm.Reading)
                return;

            var textRange = sender as TextRange;
            if (textRange != null)
            {
                //bool paragraphChanged = true;
                var start = textRange.Start;
                var end = textRange.End;
                var newWord = new TextRange(start, end);

                bool paragraphChanged = true;
                
                // Check if paragraph changed
                if (lastParagraph != null && lastParagraph.Start.CompareTo(newWord.Start.Paragraph.ContentStart) == 0
                       && lastParagraph.End.CompareTo(newWord.End.Paragraph.ContentEnd) == 0)
                {
                    paragraphChanged = false;
                }

                // Unindicate last word (and paragraph if it changed)
                unindicate(paragraphChanged);

                lastWord = new TextRange(start, end);

                // Save last value and set the new values on word (and paragraph if it changed)
                if (paragraphChanged)
                {
                    lastParagraph = new TextRange(lastWord.Start.Paragraph.ContentStart, lastWord.End.Paragraph.ContentEnd);
                    lastParaBackground = lastParagraph.GetPropertyValue(TextElement.BackgroundProperty);
                    if (lastParaBackground != null &&
                        !TextElement.BackgroundProperty.IsValidValue(lastParaBackground))
                    {
                        lastParaBackground = null;
                    }
                    lastParagraph.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                }
                lastWordBackground = lastWord.GetPropertyValue(TextElement.BackgroundProperty);
                lastWord.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightGreen);

                rtb.Selection.Select(lastWord.Start, lastWord.Start);

                srcollToSelection();
            }
        }

        private void srcollToSelection()
        {
            Rect rect = rtb.Selection.Start.GetCharacterRect(LogicalDirection.Forward);

            // if the text is in the window
            if (rect != null && rect.Y >= 0 && rect.Y < rtb.ExtentHeight && !rect.IsEmpty)
            {
                // Scroll down so text is in the middle of the window
                double y = (rtb.ViewportHeight / 2 - rect.Y - rect.Height / 2);

                rtb.ScrollToVerticalOffset(rtb.VerticalOffset - y);
            }
        }

        private void unindicate(bool paragraph)
        {
            if (lastWord != null)
            {
                lastWord.ApplyPropertyValue(TextElement.BackgroundProperty, lastWordBackground);

                if (paragraph)
                {
                    lastParagraph.ApplyPropertyValue(TextElement.BackgroundProperty, lastParaBackground);
                }
            }
        }
    }
}
