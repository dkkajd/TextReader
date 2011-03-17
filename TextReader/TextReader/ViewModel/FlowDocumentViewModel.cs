using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace TextReader.ViewModel
{
    class FlowDocumentViewModel : WorkspaceViewModel
    {

        private readonly FlowDocument _doc;
        public FlowDocument Document
        {
            get
            {
                return _doc;
            }
        }

        private string _name;
        public String Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }


        private bool _reading;
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

        private readonly TextRange _selection;
        public TextRange Selection
        {
            get { return _selection; }
        }

        private readonly TextRange _readWord;
        public TextRange ReadWord
        {
            get { return _readWord; }
        }

        static int nameCount = 0;
        static string GenerateNewName()
        {
            nameCount++; // Means we start at 1
            return "Name " + nameCount.ToString();
        }

        public FlowDocumentViewModel(FlowDocument doc)
        {
            _doc = doc;
            _name = GenerateNewName();
            _selection = new TextRange(_doc.ContentStart, _doc.ContentStart);
            _readWord = new TextRange(_doc.ContentStart, _doc.ContentStart);
            _reading = false;
        }
    }
}
