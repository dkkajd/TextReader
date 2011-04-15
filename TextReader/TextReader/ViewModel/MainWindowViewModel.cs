using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TextReader.Model;
using System.Collections.Specialized;

namespace TextReader.ViewModel
{
    class MainWindowViewModel : WorkspaceViewModel
    {
        private DocumentReader _reader;
        private ReadDocumentViewModel _readDoc;
        private ReadDocumentManager _docsMan;

        private readonly CommandBindingCollection _commandBindings;
        public CommandBindingCollection CommandBindings
        {
            get
            {
                return _commandBindings;
            }
        }

        #region FlowDocumentViewModel
        private ObservableCollection<ReadDocumentViewModel> _rdvms;
        public ReadOnlyObservableCollection<ReadDocumentViewModel> Documents
        {
            get { return new ReadOnlyObservableCollection<ReadDocumentViewModel>(_rdvms); }
        }

        private ReadDocumentViewModel _doc;
        public ReadDocumentViewModel Document
        {
            get { return _doc; }
            set
            {
                if (value == null || Documents.Contains(value))
                {
                    _doc = value;
                    OnPropertyChanged("Document");
                }
            }
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
                else
                {
                    OnPropertyChanged("Voice");
                }
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
                else
                {
                    OnPropertyChanged("Rate");
                }
            }
        }

        public int Volume
        {
            get { return _reader.GetVolume(); }
            set
            {
                var temp = Volume;
                _reader.SetVolume(value);
                if (temp == Volume)
                {
                    // We need to tell it that it never changed in a new thread or combobox wont update
                    Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate
                    {
                        OnPropertyChanged("Volume");
                    });
                }
                else
                {
                    OnPropertyChanged("Volume");
                }
            }
        }
        #endregion

        public MainWindowViewModel(ReadDocumentManager docsMan)
        {

            // Add commandbindings
            _reader = new DocumentReader();
            _reader.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_reader_PropertyChanged);

            _commandBindings = new CommandBindingCollection();

            CommandBinding NewCmdBinding = new CommandBinding(
                ApplicationCommands.New, (t, e) => _docsMan.Add());
            this.CommandBindings.Add(NewCmdBinding);

            CommandBinding PlayCmdBinding = new CommandBinding(
                MediaCommands.Play, PlayCmdExecuted, PlayCmdCanExecute);
            this.CommandBindings.Add(PlayCmdBinding);

            CommandBinding PauseCmdBinding = new CommandBinding(
                MediaCommands.Pause, PauseCmdExecuted, PauseCmdCanExecute);
            this.CommandBindings.Add(PauseCmdBinding);

            CommandBinding StopCmdBinding = new CommandBinding(
                MediaCommands.Stop, (t, e) => _reader.StopReading());
            this.CommandBindings.Add(StopCmdBinding);


            _docsMan = docsMan;
            _rdvms = new ObservableCollection<ReadDocumentViewModel>();
            
            //The easiest way to republate the viewmodel is to call the delegate that manages the changes
            MainWindowViewModel_CollectionChanged(_docsMan.Documents, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            // To keep our ViewModelCollection up to date we need to listen for changes
            ((INotifyCollectionChanged)(_docsMan.Documents)).CollectionChanged += new NotifyCollectionChangedEventHandler(MainWindowViewModel_CollectionChanged);

            // Select the first document if any.
            Document = Documents.FirstOrDefault();
        }

        void MainWindowViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        addDocToRDVMs((ReadDocument)item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Remove the viewmodel that has n underlying object that is removed.
                    var itemsToRemove = from rd in _rdvms where (e.OldItems.Contains(rd.GetUnderlyingObject())) select rd;
                    foreach (var item in itemsToRemove.ToList())
	                {
                        tryRemoveRDVM(item); 
	                }
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    _rdvms.Clear();
                    foreach (var item in ((ReadOnlyCollection<ReadDocument>)sender))
                    {
                        addDocToRDVMs(item);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper function that tries to remove a ReadDocumentViewModel.
        /// </summary>
        /// <param name="rdvm">The ReadDocumentViewModel</param>
        private void tryRemoveRDVM(ReadDocumentViewModel rdvm)
        {
            if (rdvm == null)
                return;

            // Stop reading if it was reading the closing document
            if (_readDoc == rdvm)
                _reader.StopReading();

            // Remove the viewmodel if it exists
            if (_rdvms.Contains(rdvm))
            {
                _rdvms.Remove(rdvm);
            }
        }

        private void addDocToRDVMs(ReadDocument doc)
        {
            var docvm = new ReadDocumentViewModel(doc);
            _rdvms.Add(docvm);

            // When remove is requested remove the document from the manager, whih will inturn remove the docvm
            docvm.RequestClose = (s, e) => { _docsMan.Remove(doc); };
            
            //set the new document as selected
            Document = docvm;
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
                case "Volume":
                    // Relay that the property have changed
                    OnPropertyChanged("Volume");
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
                            CommandManager.InvalidateRequerySuggested();
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

        void PlayCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            _readDoc = _doc;
            _reader.StartReading(_readDoc.Selection.Start);
            updateReadText();
        }
        [DebuggerStepThrough]
        void PlayCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Allow start reading only when it's not already reading, or it have been paused.
            // And there actually is a document to read.
            e.CanExecute = _doc != null && (_reader.State == ReaderState.NotSpeaking || _reader.State == ReaderState.Paused);
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
        [DebuggerStepThrough]
        void PauseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _reader.State == ReaderState.Speaking ||
                _reader.State == ReaderState.Paused;
        }


        private ICommand _removeEmptyCommand;
        public ICommand RemoveEmptyCommand
        {
            get
            {
                if (_removeEmptyCommand == null)
                {
                    _removeEmptyCommand = new RelayCommand(RemoveEmptyCmdExecuted, RemoveEmptyCmdCanExecute);
                }
                return _removeEmptyCommand;
            }
        }

        void RemoveEmptyCmdExecuted(object target)
        {
            Document.RemoveEmptyLines();
        }
        [DebuggerStepThrough]
        bool RemoveEmptyCmdCanExecute(object sender)
        {
            return Document != null && !Document.Reading;
        }

        private ICommand _uniteLinesCommand;
        public ICommand UniteLinesCommand
        {
            get
            {
                if (_uniteLinesCommand == null)
                {
                    _uniteLinesCommand = new RelayCommand(UniteLinesCmdExecuted, UniteLinesCmdCanExecute);
                }
                return _uniteLinesCommand;
            }
        }

        void UniteLinesCmdExecuted(object target)
        {
            Document.UniteLines();
        }
        [DebuggerStepThrough]
        bool UniteLinesCmdCanExecute(object sender)
        {
            return Document != null && !Document.Reading;
        }
        #endregion
    }
}
