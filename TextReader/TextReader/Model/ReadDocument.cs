using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;

namespace TextReader.Model
{
    class ReadDocument : INotifyPropertyChanged
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
            set
            {
                if (value != _name)
                {
                    _name = value; 
                    OnPropertyChanged("Name");
                } 
            }
        }

        private double _scrollOffset;

        public double ScrollOffset
        {
            get { return _scrollOffset; }
            set {
                if (value != _scrollOffset)
                {
                    _scrollOffset = value; 
                    OnPropertyChanged("ScrollOffset");
                }
            }
        }

        private readonly TextRange _selection;
        public TextRange Selection
        {
            get {
                AssureRightDocument(_selection);
                return _selection;
            }
        }
        
        /// <summary>
        /// Checks the textrange to make sure it's still inside the document,
        /// and if not it selects the start of the document.
        /// </summary>
        /// <param name="range">The TextRange to check.</param>
        private void AssureRightDocument(TextRange range)
        {
            if (!range.Start.IsInSameDocument(_doc.ContentStart))
            {
                range.Select(_doc.ContentStart, _doc.ContentStart);
            }
        }

        static int nameCount = 0;
        static string GenerateNewName()
        {
            nameCount++; // Means we start at 1
            return "Name " + nameCount.ToString();
        }

        public ReadDocument(FlowDocument doc)
        {
            _doc = doc;
            _scrollOffset = 0;
            _name = GenerateNewName();
            _selection = new TextRange(_doc.ContentStart, _doc.ContentStart);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void RemoveEmptyLines()
        {
            // Set start an undo command if we can
            TextBoxBase textBox = _doc.Parent as TextBoxBase;
            if (textBox != null)
	            textBox.BeginChange();

            var blocksToRemove = new List<Block>();
            foreach (var block in _doc.Blocks)
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
                _doc.Blocks.Remove(block);
            }

            // End the undo command
            if (textBox != null)
                textBox.EndChange();
        }

        public void UniteLines()
        {
            // Set start an undo command if we can
            TextBoxBase textBox = _doc.Parent as TextBoxBase;
            if (textBox != null)
                textBox.BeginChange();


            var blocksToRemove = new List<Block>();
            Paragraph lastPara = null;
            var block = _doc.Blocks.FirstBlock;

            while (block != null)
            {
                var para = block as Paragraph;
                if (para != null)
                {
                    if (para.Inlines.Count == 1)
                    {
                        var run = para.Inlines.FirstInline as Run;
                        if (run != null && run.Text.Trim() == "")
                        {
                            lastPara = para;
                            block = block.NextBlock;
                            continue;
                        }
                    }
                    if (para.Inlines.Count >= 1)
                    {
                        if (lastPara == null)
                        {
                            lastPara = para;
                        }
                        else
                        {
                            para.ContentStart.GetInsertionPosition(LogicalDirection.Forward).InsertTextInRun(" ");
                            var inlines = para.Inlines.Take(para.Inlines.Count);

                            lastPara.Inlines.AddRange(inlines);
                            blocksToRemove.Add(para);
                        }
                    }
                }
                block = block.NextBlock;
            }


            foreach (var item in blocksToRemove)
            {
                _doc.Blocks.Remove(item);
            }

            // End the undo command
            if (textBox != null)
                textBox.EndChange();
        }
    }
}
