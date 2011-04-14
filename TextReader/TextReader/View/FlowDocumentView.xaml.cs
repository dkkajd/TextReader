﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using TextReader.ViewModel;
using System.Windows.Threading;
using System.ComponentModel;

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
            // Save the offset on the old flowdocument
            var oldFdvm = e.OldValue as ReadDocumentViewModel;
            if (oldFdvm != null)
            {
                oldFdvm.ScrollOffset = rtb.VerticalOffset;
            }

            var fdvm = (DataContext as ReadDocumentViewModel);
            if (fdvm == null)
                return;
            // if for somereason the document already have a parent we need to remove that first
            if (fdvm.Document.Parent != null)
            {
                var doc = new FlowDocument();
                ((RichTextBox)fdvm.Document.Parent).Document = doc;
            }
            rtb.Document = fdvm.Document;

            rtb.Selection.Select(fdvm.Selection.Start, fdvm.Selection.End);

            // sets the vertical after it has scrolled for the selection
            Dispatcher.BeginInvoke((Action)(() => { rtb.ScrollToVerticalOffset(fdvm.ScrollOffset); }), DispatcherPriority.Input, null);

            rtb.Selection.Changed += new EventHandler(Selection_Changed);
            fdvm.ReadWord.Changed += new EventHandler(ReadWord_Changed);
            fdvm.PropertyChanged += new PropertyChangedEventHandler(fdvm_PropertyChanged);
            Keyboard.Focus(rtb);
        }

        void fdvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Reading":
                    var fdvm = (DataContext as ReadDocumentViewModel);
                    if (fdvm == null)
                        return;
                    
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
            var fdvm = (DataContext as ReadDocumentViewModel);
            if (fdvm == null)
                return;
            fdvm.Selection.Select(rtb.Selection.Start, rtb.Selection.End);
        }

        TextRange lastWord;
        object lastWordBackground;
        TextRange lastParagraph;
        object lastParaBackground;
        void ReadWord_Changed(object sender, EventArgs e)
        {
            var fdvm = (DataContext as ReadDocumentViewModel);
            if (fdvm == null)
                return;

            // Only do anything if it's being read
            if (!fdvm.Reading)
                return;

            var textRange = sender as TextRange;
            if (textRange != null)
            {
                var start = textRange.Start;
                var end = textRange.End;
                var newWord = new TextRange(start, end);

                bool paragraphChanged = true;
                
                // Check if paragraph changed
                if (lastParagraph != null && lastParagraph.Start.IsInSameDocument(start) && lastParagraph.Start.CompareTo(newWord.Start.Paragraph.ContentStart) == 0
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

        private void rtb_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(rtb);
        }
    }
}
