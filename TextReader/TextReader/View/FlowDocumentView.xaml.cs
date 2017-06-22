using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using TextReader.ViewModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Diagnostics;

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

                    rtb.IsDocumentEnabled = false;

                    if (!fdvm.Reading)
                    {
                        //unindicate(true);
                        lastWord = null;
                        lastParagraph = null;
                        rtb.IsDocumentEnabled = true;
                        clearHighLightsWord();
                        clearHighLightsPara();
                        setHighLights();
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

        TextRange lastParagraph;

        TextRange newRead = null;
        TextRange lastRead = null;

        void ReadWord_Changed2()
        {
            if (newRead == null)
                return;

            TextRange nowRead = new TextRange(newRead.Start,newRead.End);
            if (lastRead != null 
                && nowRead.Start.CompareTo(lastRead.Start) == 0 
                && nowRead.End.CompareTo(lastRead.End) == 0)
                return;

            lastRead = new TextRange(nowRead.Start, nowRead.End);


            var start = nowRead.Start;
            var end = nowRead.End;
            var newWord = new TextRange(start, end);

            bool paragraphChanged = true;

            // Check if paragraph changed
            if (lastParagraph != null && lastParagraph.Start.IsInSameDocument(start) && lastParagraph.Start.CompareTo(newWord.Start.Paragraph.ContentStart) == 0
                   && lastParagraph.End.CompareTo(newWord.End.Paragraph.ContentEnd) == 0)
            {
                paragraphChanged = false;
            }

            lastWord = new TextRange(start, end);

            // Save last value and set the new values on word (and paragraph if it changed)
            if (paragraphChanged)
            {
                lastParagraph = new TextRange(lastWord.Start.Paragraph.ContentStart, lastWord.End.Paragraph.ContentEnd);
            }

            //setHighLights();

            rtb.Selection.Select(lastWord.Start, lastWord.Start);

            srcollToSelection();


        }

        private delegate void ZeroArgDelegate();

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
                newRead = textRange;
                Dispatcher.BeginInvoke(new ZeroArgDelegate(ReadWord_Changed2), DispatcherPriority.Render, null);
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
            setHighLights();
        }

        private void addTextPointerRects(Panel panel, TextRange range, Brush brush)
        {
            if (range == null)
                return;

            //TextPointer s = range.Start;

            var res = range.Start;

            var rectS = res.GetCharacterRect(LogicalDirection.Forward);

            var shapes = new System.Collections.Generic.LinkedList<Shape>();
            Action<TextPointer, TextPointer> instert = ( (tr,tr2) =>
            {
                //var rectS = tr.GetCharacterRect(LogicalDirection.Forward);
                //var tr2 = tr.GetNextInsertionPosition(LogicalDirection.Forward);
                var rectE = tr2.GetCharacterRect(LogicalDirection.Backward);

                // Do not add if on a new line
                if (rectS.Top != rectE.Top)
                {
                    rectS = tr.GetCharacterRect(LogicalDirection.Forward);
                }
                // Do not add if on a new line
                if (rectS.Top == rectE.Top)
                {
                    try
                    {

                        var shape = new Rectangle()
                        {
                            Margin = new Thickness(rectS.Left, rectS.Top, 0, 0),
                            Height = rectS.Height,
                            Width = rectE.Right - rectS.Left,
                            Fill = brush,
                            //StrokeThickness = 1,
                            //Stroke = Brushes.Black,
                        };
                        shapes.AddLast(shape);
                    }
                    catch (Exception)
                    {
                    }
                }
                rectS = rectE;
            });


            // we simly travers the textpoint untill we count reaches 0
            while (res != null && range.End.CompareTo(res) > 0)
            {
                var next = res.GetNextInsertionPosition(LogicalDirection.Forward);
                //var next = res.GetNextContextPosition(LogicalDirection.Forward);
                instert(res,next);
                res = next;
            }

            foreach (var s in shapes)
            {
                panel.Children.Add(s);
            }
        }

        
        int i = 0;

        private void setHighLights()
        {
            setHighLightsPara();
            setHighLightsWord();
        }

        Rect lastStartParaRect = new Rect();

        private void clearHighLightsPara()
        {
            lastStartParaRect = new Rect();
            if (hlPara == null)
                return;
            hlPara.Children.Clear();
        }

        private void setHighLightsPara()
        {
            if (hlPara == null)
                return;
            if (lastParagraph == null)
                return;
            if (lastStartParaRect == lastParagraph.Start.GetCharacterRect(LogicalDirection.Forward))
                return;

            clearHighLightsPara();

            lastStartParaRect = lastParagraph.Start.GetCharacterRect(LogicalDirection.Forward);

            Debug.WriteLine(i++);

            addTextPointerRects(hlPara, lastParagraph, Brushes.Yellow);

        }

        Rect lastStartRect = new Rect();

        private void clearHighLightsWord()
        {
            lastStartRect = new Rect();
            if (hl == null)
                return;
            hl.Children.Clear();
        }

        private void setHighLightsWord()
        {
            if (hl == null)
                return;
            if (lastWord == null)
                return;
            if (lastStartRect == lastWord.Start.GetCharacterRect(LogicalDirection.Forward))
                return;
            clearHighLightsWord();
            lastStartRect = lastWord.Start.GetCharacterRect(LogicalDirection.Forward);


            addTextPointerRects(hl, lastWord, Brushes.LightGreen);

        }

        private void rtb_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(rtb);
        }


        Polygon wb = null;
        private void WordBorder_Loaded(object sender, RoutedEventArgs e)
        {
            wb = sender as Polygon;
        }

        Canvas hl = null;
        private void HighLights_Loaded(object sender, RoutedEventArgs e)
        {
            hlPara = sender as Canvas;
        }

        private void rtb_LayoutUpdated(object sender, EventArgs e)
        {
            setHighLights();
        }

        Canvas hlPara = null;
        private void HighLightsWord_Loaded(object sender, RoutedEventArgs e)
        {
            hl = sender as Canvas;

        }
    }
}
