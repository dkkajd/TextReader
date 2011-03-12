using TextReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Documents;

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
    }
}
