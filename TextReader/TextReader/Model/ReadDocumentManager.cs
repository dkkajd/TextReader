using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Reflection;

namespace TextReader.Model
{
    class ReadDocumentManager
    {
        private readonly ObservableCollection<ReadDocument> _docs;

        public ReadOnlyObservableCollection<ReadDocument> Documents
        {
            get { return new ReadOnlyObservableCollection<ReadDocument>(_docs); }
        }

        #region Constructors
        /// <summary>
        /// Creates a ReadDocumentManager with no starting documents.
        /// </summary>
        public ReadDocumentManager() : this(Enumerable.Empty<ReadDocument>())
        {
        }

        /// <summary>
        /// Creates an ReadDocumentManager with the specified starting documents.
        /// </summary>
        /// <param name="startDocuments">The ReadDocuments it starts with.</param>
        public ReadDocumentManager(IEnumerable<ReadDocument> startDocuments)
        {
            _docs = new ObservableCollection<ReadDocument>(startDocuments);
        }
        #endregion

        /// <summary>
        /// Adds a ReadDocument to the collection.
        /// </summary>
        /// <returns>The new ReadDocument that is added.</returns>
        public ReadDocument Add()
        {
            return Add(new FlowDocument());
        }

        /// <summary>
        /// A private helper function that creates a ReadDocument from a FlowDocument and adds it to the collection.
        /// </summary>
        /// <param name="flowDoc">The FlowDocument used to create the new ReadDocument.</param>
        /// <returns>The added ReadDocument.</returns>
        private ReadDocument Add(FlowDocument flowDoc)
        {
            var res = new ReadDocument(flowDoc);
            _docs.Add(res);
            return res;
        }

        /// <summary>
        /// Removes a ReadDocument from the collection.
        /// </summary>
        /// <param name="doc">The ReadDocument that is removed.</param>
        public void Remove(ReadDocument doc)
        {
            _docs.Remove(doc);
        }
    }
}
