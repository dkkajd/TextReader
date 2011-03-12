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
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;

namespace TextReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DocumentReader _reader;

        Dictionary<ReaderState, String> StateToText = new Dictionary<ReaderState, string>{
                    {ReaderState.NotSpeaking, "Stopped"},
                    {ReaderState.Paused, "Paused"},
                    {ReaderState.Speaking, "Speaking"},
                    {ReaderState.StartedSpeaking, "Started speaking"}};

        public MainWindow()
        {
            InitializeComponent();
            _reader = new DocumentReader();
            _reader.PropertyChanged += new PropertyChangedEventHandler(_reader_PropertyChanged);

            this.DataContext = _reader;

            comboBox1.ItemsSource=_reader.GetVoices();

            // set the fields
            txtStatus.Content = StateToText[_reader.State];
            sldSpeakSpeed.Value = _reader.GetRate();
            comboBox1.SelectedItem = _reader.GetVoice();

            // Add commandbindings

            CommandBinding PlayCmdBinding = new CommandBinding(
                MediaCommands.Play, PlayCmdExecuted, PlayCmdCanExecute);
            this.CommandBindings.Add(PlayCmdBinding);

            CommandBinding PauseCmdBinding = new CommandBinding(
                MediaCommands.Pause, PauseCmdExecuted, PauseCanExecute);
            this.CommandBindings.Add(PauseCmdBinding);

            CommandBinding StopCmdBinding = new CommandBinding(
                MediaCommands.Stop, StopCmdExecuted, CanExecuteTrue);
            this.CommandBindings.Add(StopCmdBinding);

            CommandBinding RemoveEmptyCmdBinding = new CommandBinding(
                (RoutedCommand)Resources["RemoveEmptyCommand"], RemoveEmptyCmdExecuted, CanExecuteTrue);
            this.CommandBindings.Add(RemoveEmptyCmdBinding);

            CommandBinding HomeCmdBinding = new CommandBinding(
                (RoutedCommand)Resources["HomeCommand"], HomeCmdExecuted, CanExecuteTrue);
            this.CommandBindings.Add(HomeCmdBinding);

            CommandBinding EndCmdBinding = new CommandBinding(
                (RoutedCommand)Resources["EndCommand"], EndCmdExecuted, CanExecuteTrue);
            this.CommandBindings.Add(EndCmdBinding);
        }

        void CanExecuteTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void PlayCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            _reader.StartReading(rtb.Selection.Start.GetLineStartPosition(0));
            lastWord = _reader.ReadText;
        }
        void PlayCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _reader.State == ReaderState.NotSpeaking;
        }
        void PauseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            switch (_reader.State)
            {   case ReaderState.Speaking:
                    _reader.PauseReading();
                    break;
                case ReaderState.Paused:
                    _reader.ResumeReading();
                    break;
            }
        }
        void PauseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _reader.State == ReaderState.Speaking ||
                _reader.State == ReaderState.Paused;
        }
        void StopCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            _reader.StopReading();
        }
        void RemoveEmptyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var blocksToRemove = new List<Block>();
            foreach (var block in rtb.Document.Blocks)
            {
                var para = block as Paragraph;
                if (para != null && para.Inlines.Count == 1)
                {
                    var run = para.Inlines.FirstInline as Run;
                    if (run != null && run.Text == "")
                    {
                        blocksToRemove.Add(block);
                    }
                }
            }

            foreach (var block in blocksToRemove)
            {
                rtb.Document.Blocks.Remove(block);
            }
        }
        void HomeCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
        }
        void EndCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            rtb.Selection.Select(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
        }
        
        
        TextRange lastWord;
        void _reader_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var inst = (DocumentReader)sender;
            if (e.PropertyName=="ReadText")
            {
                unindicateWord();
                TextPointer start = inst.ReadText.Start;
                TextPointer end = inst.ReadText.End;

                TextRange lastParagraph = new TextRange(inst.ReadText.Start.Paragraph.ContentStart, end.Paragraph.ContentEnd);
                lastParagraph.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);

                rtb.Selection.Select(start, start);
                lastWord = new TextRange(start, end);
                lastWord.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightGreen);

                Rect rect = start.GetCharacterRect(LogicalDirection.Forward);

                // if the text is in the window
                if (rect != null && rect.Y >= 0 && rect.Y < rtb.ExtentHeight &&!rect.IsEmpty)
                {
                    // Scroll down so text is in the middle of the window
                    double y = (rtb.ViewportHeight / 2 - rect.Y-rect.Height/2);
                    
                    rtb.ScrollToVerticalOffset(rtb.VerticalOffset - y);
                }
                
            }
            else if (e.PropertyName == "State")
            {
                txtStatus.Content = StateToText[inst.State];
                if (_reader.State == ReaderState.NotSpeaking)
                {
                    unindicateWord();
                    // When we sop speaking thre's no long a 
                    // need to remeber which word was read last
                    lastWord = null;
                }
            }
            else if (e.PropertyName == "Rate")
            {
                sldSpeakSpeed.Value = _reader.GetRate();
            }
            this.UpdateLayout();
        }

        private void unindicateWord()
        {
            if (lastWord == null)
                return;

            // not needed, removing it with the paragraph
            //// unindicated the last word
            //lastWord.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            
            //if (lastWord.Start.Paragraph != null && lastWord.End.Paragraph != null)
            {
                TextRange lastParagraph = new TextRange(lastWord.Start.Paragraph.ContentStart, lastWord.End.Paragraph.ContentEnd);
                if (lastParagraph != null)
                    lastParagraph.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBox1.SelectedItem = _reader.SetVoice((String)comboBox1.SelectedItem );
        }

        private void sldSpeakSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _reader.SetRate((int)e.NewValue);
        }

    }
}
