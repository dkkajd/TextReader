using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace TextReader.ViewModel
{
    class MainWindowViewModel : WorkspaceViewModel
    {
        private DocumentReader _reader;
        private FlowDocumentViewModel _readDoc;

        private readonly CommandBindingCollection _commandBindings;
        public CommandBindingCollection CommandBindings
        {
            get
            {
                return _commandBindings;
            }
        }


        #region FlowDocumentViewModel
        private readonly List<FlowDocumentViewModel> _docs;
        public IEnumerable<FlowDocumentViewModel> Documents
        {
            get { return _docs; }
        }

        private FlowDocumentViewModel _doc;
        public FlowDocumentViewModel Document
        {
            get { return _doc; }
            set { _doc = value; }
        }
        #endregion


        #region Relay of DocumentReader properties
        public IEnumerable<string> Voices
        {
            get { return _reader.GetVoices(); }

        }

        public string Voice
        {
            get { return _reader.GetVoice(); }
            set
            {
                var temp = Voice;
                _reader.SetVoice(value);
                if (temp == Voice)
                {
                    // We need to tell it that it never changed in a new thread or combobox wont update
                    Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate
                    {
                        OnPropertyChanged("Voice");
                    });
                }
                OnPropertyChanged("Voice");
            }
        }

        public int Rate
        {
            get { return _reader.GetRate(); }
            set
            {
                var temp = Rate;
                _reader.SetRate(value);
                if (temp == Rate)
                {
                    // We need to tell it that it never changed in a new thread or combobox wont update
                    Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate
                    {
                        OnPropertyChanged("Rate");
                    });
                }
                OnPropertyChanged("Rate");
            }
        }
        #endregion

        public MainWindowViewModel()
        {

            // Add commandbindings
            _reader = new DocumentReader();
            _reader.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_reader_PropertyChanged);

            _commandBindings = new CommandBindingCollection();

            CommandBinding PlayCmdBinding = new CommandBinding(
                MediaCommands.Play, PlayCmdExecuted, PlayCmdCanExecute);
            this.CommandBindings.Add(PlayCmdBinding);

            CommandBinding PauseCmdBinding = new CommandBinding(
                MediaCommands.Pause, PauseCmdExecuted, PauseCanExecute);
            this.CommandBindings.Add(PauseCmdBinding);

            CommandBinding StopCmdBinding = new CommandBinding(
                MediaCommands.Stop, StopCmdExecuted, CanExecuteTrue);
            this.CommandBindings.Add(StopCmdBinding);

            // Set the first document to the welcome text
            FlowDocument doc = ((FlowDocument)App.Current.Resources["WelcomeDocument"]);
            _docs = new List<FlowDocumentViewModel>();
            _docs.Add(new FlowDocumentViewModel(doc));
            Document = Documents.First();
        }

        void _reader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Voice":
                    // Relay that the property have changed
                    OnPropertyChanged("Voice");
                    break;
                case "Rate":
                    // Relay that the property have changed
                    OnPropertyChanged("Rate");
                    break;
                case "ReadText":
                    updateReadText();
                    break;
                case "State":
                    switch (_reader.State)
                    {
                        case ReaderState.StartedSpeaking:
                            break;
                        case ReaderState.Speaking:
                            _readDoc.Reading = true;
                            break;
                        case ReaderState.Paused:
                            break;
                        case ReaderState.NotSpeaking:
                            _readDoc.Reading = false;
                            CommandManager.InvalidateRequerySuggested();
                            break;
                    }
                    break;
            }
        }

        void updateReadText()
        {
            if (_reader.ReadText != null)
            {
                _reader.ReadText.Changed += new EventHandler(ReadText_Changed);
                ReadText_Changed(null, null);
            }
        }

        void ReadText_Changed(object sender, EventArgs e)
        {
            if (_reader.ReadText != null)
            {
                _readDoc.ReadWord.Select(_reader.ReadText.Start, _reader.ReadText.End);
            }
        }


        #region Commands

        void CanExecuteTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void PlayCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            _readDoc = _doc;
            _reader.StartReading(_readDoc.Selection.Start);
            updateReadText();
        }
        void PlayCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _reader.State == ReaderState.NotSpeaking;
        }
        void PauseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            switch (_reader.State)
            {
                case ReaderState.Speaking:
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


        private ICommand _removeEmptyCommand;
        public ICommand RemoveEmptyCommand
        {
            get
            {
                if (_removeEmptyCommand == null)
                {
                    _removeEmptyCommand = new RelayCommand(RemoveEmptyCmdExecuted,RemoveEmptyCmdCanExecute);
                }
                return _removeEmptyCommand;
            }
        }

        void RemoveEmptyCmdExecuted(object target)
        {
            var blocksToRemove = new List<Block>();
            foreach (var block in Document.Document.Blocks)
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
                Document.Document.Blocks.Remove(block);
            }
        }
        bool RemoveEmptyCmdCanExecute(object sender)
        {
            return Document != null;
        }
        #endregion
    }
}
