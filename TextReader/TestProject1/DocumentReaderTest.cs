using TextReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Threading;

namespace TestTextReader
{
    
    
    /// <summary>
    ///This is a test class for DocumentReaderTest and is intended
    ///to contain all DocumentReaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DocumentReaderTest
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


        /// <summary>
        ///A test for GetPositionAtTextOffset
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TextReader.exe")]
        public void GetPositionAtTextOffsetTest()
        {
            // Set up a document
            var doc = new FlowDocument();
            var line01 = new Run("Hey");
            var line02 = new Run("Yo there");
            var line03 = new Run("");
            var line04 = new Run("I'm good");
            doc.Blocks.Add(new Paragraph(line01));
            doc.Blocks.Add(new Paragraph(line02));
            doc.Blocks.Add(new Paragraph(line03));
            doc.Blocks.Add(new Paragraph(line04));

            TextPointer source;
            TextPointer expected;
            TextPointer actual;
            int count;

            // Test the start
            source = doc.ContentStart;
            expected = line01.ContentStart;
            count = 0;

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test for next line
            source = doc.ContentStart;
            expected = line02.ContentStart;
            count = 5; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test to empty line
            source = doc.ContentStart;
            expected = line03.ContentStart;
            count = 15; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test to after empty line
            source = doc.ContentStart;
            expected = line04.ContentStart;
            count = 17; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test to end
            source = doc.ContentStart;
            expected = line04.ContentEnd;
            count = 17 + 8;

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test from 2nd Paragraph
            // Test the start
            source = line02.ContentStart;
            expected = line02.ContentStart;
            count = 0;

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test for next line
            source = line02.ContentStart;
            expected = line03.ContentStart;
            count = 10; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test to after emptyline
            source = line02.ContentStart;
            expected = line04.ContentStart;
            count = 12; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

            // Test to end
            source = line02.ContentStart;
            expected = line04.ContentEnd;
            count = 12+8; // Start of next line (+ the '\n\r')

            actual = DocumentReader_Accessor.GetPositionAtTextOffset(source, count);
            Assert.AreEqual(actual.CompareTo(expected), 0);

        }



        Dictionary<TextPointer,String> readWords;

        /// <summary>
        ///A test for _synth_SpeakProgress
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TextReader.exe")]
        public void _synth_SpeakProgressTest()
        {
            DocumentReader dr = new DocumentReader();
            dr.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(dr_PropertyChanged);

            // Set up a document
            var doc = new FlowDocument();
            var line01 = new Run("Hey");
            var line02 = new Run("Yo there");
            var line03 = new Run("");
            var line04 = new Run("I'm good");
            doc.Blocks.Add(new Paragraph(line01));
            doc.Blocks.Add(new Paragraph(line02));
            doc.Blocks.Add(new Paragraph(line03));
            doc.Blocks.Add(new Paragraph(line04));

            readWords = new Dictionary<TextPointer, string>();

            dr.StartReading(doc.ContentStart);

            DateTime startTime = DateTime.Now;
            TimeSpan timeout = new TimeSpan(0,0,30);
            while (dr.State != ReaderState.NotSpeaking && DateTime.Now.Subtract(startTime).CompareTo(timeout) < 0)
            {
                Thread.Sleep(new TimeSpan(0,0,2));
            }
            if (dr.State != ReaderState.NotSpeaking)
            {
                Assert.Inconclusive("Reader didn't stop in time, so inconclusive.");
            }
            if (readWords.Count != 6)
            {
                Assert.Fail("Too meny or few words read");
            }
            // tests that it is ral words
            // don't really test that it read the right words.
            foreach (var word in readWords)
            {
                if (!word.Key.IsInSameDocument(doc.ContentStart))
                {
                    Assert.Fail("Return an invalid word");
                }

                char[] textBuffer = new char[word.Value.Length];
                word.Key.GetTextInRun(LogicalDirection.Forward,textBuffer,0,word.Value.Length);
                var value = new String(textBuffer);
                if (word.Value != value)
                {
                    Assert.Fail("Read a wrong word");
                }
            }
        }

        void dr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var dr = sender as DocumentReader;
            if (sender != null)
            {
                if (e.PropertyName == "ReadText")
                {
                    readWords.Add(dr.ReadText.Start, dr.ReadText.Text);
                }
            }
        }
    }
}
