using System;
using System.Windows.Documents;
using TextReader.Model;

namespace TextReader.ViewModel
{
    class ReadDocumentViewModel : WorkspaceViewModel
    {

        private readonly ReadDocument _doc;

#region Relay of ReadDocument properties and methods
        /// <summary>
        /// The FlowDocument containing the text to be read.
        /// </summary>
        public FlowDocument Document
        {
            get
            {
                return _doc.Document;
            }
        }

        /// <summary>
        /// The name of the document.
        /// This property is watched by INotifyPropertyChanged.
        /// </summary>
        public String Name
        {
            get { return _doc.Name; }
            set { _doc.Name = value; }
        }

        /// <summary>
        /// The offset the viewer has at the moment.
        /// This property is watched by INotifyPropertyChanged.
        /// </summary>
        public double ScrollOffset
        {
            get { return _doc.ScrollOffset; }
            set { _doc.ScrollOffset = value; }
        }

        /// <summary>
        /// What is currently selected in the document.
        /// </summary>
        public TextRange Selection
        {
            get { return _doc.Selection; }
        }

        void _doc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                case "OffSet":
                case "":
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        public void RemoveEmptyLines()
        {
            _doc.RemoveEmptyLines();
        }

        public void UniteLines()
        {
            _doc.UniteLines();
        }

#endregion

        private readonly TextRange _readWord;
        /// <summary>
        /// The word that is currently being read.
        /// </summary>
        public TextRange ReadWord
        {
            get
            {
                AssureRightDocument(_readWord);
                return _readWord;
            }
        }

        /// <summary>
        /// Checks the textrange to make sure it's still inside the document,
        /// and if not it selects the start of the document.
        /// </summary>
        /// <param name="range">The TextRange to check.</param>
        private void AssureRightDocument(TextRange range)
        {
            if (!range.Start.IsInSameDocument(_doc.Document.ContentStart))
            {
                range.Select(_doc.Document.ContentStart, _doc.Document.ContentStart);
            }
        }

        private bool _reading;
        /// <summary>
        /// Is set to true by it's controller when there is being read from it.
        /// This property is watched by INotifyPropertyChanged.
        /// </summary>
        public Boolean Reading
        {
            get { return _reading; }
            set
            {
                // only change if value is changed
                if (_reading == value)
                    return;
                _reading = value;
                OnPropertyChanged("Reading");
            }
        }

        public ReadDocumentViewModel(ReadDocument doc)
        {
            _doc = doc;
            // Watch for property changes in the ReadDocument so they can be send along.
            _doc.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_doc_PropertyChanged);
            _readWord = new TextRange(_doc.Document.ContentStart, _doc.Document.ContentStart);
            _reading = false;
        }

        internal ReadDocument GetUnderlyingObject()
        {
            return _doc;
        }
    }
}
