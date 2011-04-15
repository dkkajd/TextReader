using TextReader.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace TestTextReader
{
    
    
    /// <summary>
    ///This is a test class for ReadDocumentManagerTest and is intended
    ///to contain all ReadDocumentManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ReadDocumentManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private List<ReadDocument> createDocList()
        {
            // Test with collection
            List<ReadDocument> list = new List<ReadDocument>()
            {
                new ReadDocument(new FlowDocument()) {Name="My name"},
                new ReadDocument(new FlowDocument())
            };
            return list;
        }

        /// <summary>
        ///A test for ReadDocumentManager Constructor
        ///</summary>
        [TestMethod()]
        public void ReadDocumentManagerConstructorTest()
        {
            // Test the empty creator
            ReadDocumentManager target = new ReadDocumentManager();
            Assert.AreEqual(0, target.Documents.Count);

            var docs = createDocList();
            target = new ReadDocumentManager(docs);
            Assert.AreEqual(2, target.Documents.Count);

            CollectionAssert.AreEquivalent(docs, target.Documents);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            var docs = createDocList();
            ReadDocumentManager target = new ReadDocumentManager(docs);
            target.Add();
            var difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 1);
            target.Add();
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 2);
        }

        /// <summary>
        ///A test for Remove
        ///</summary>
        [TestMethod()]
        public void RemoveTest()
        {
            ReadDocumentManager target; // The manager that is being tested
            List<ReadDocument> docs; // The documents in the start
            ReadDocument doc1; // The created documents
            ReadDocument doc2; // The created documents
            IEnumerable<ReadDocument> difference; // Creates the difference from the before to the current

            // Test adding one document
            docs = createDocList();
            target = new ReadDocumentManager(docs);
            doc1 = target.Add();
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 1);
            Assert.AreEqual(doc1, difference.First());
            target.Remove(doc1);
            CollectionAssert.AreEqual(docs, target.Documents);

            // Test adding two documents, remove the first, and the second
            docs = createDocList();
            target = new ReadDocumentManager(docs);
            doc1 = target.Add();
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 1);
            Assert.AreEqual(doc1, difference.First());
            doc2 = target.Add();
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 2);
            target.Remove(doc1);
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 1);
            Assert.AreEqual(doc2, difference.First());
            target.Remove(doc2);
            difference = target.Documents.Except(docs);
            Assert.IsTrue(difference.Count() == 0);
            CollectionAssert.AreEqual(docs, target.Documents);
        }
    }
}
