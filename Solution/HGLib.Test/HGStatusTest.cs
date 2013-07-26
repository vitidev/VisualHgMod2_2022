using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace HGLib.Test
{
    /// <summary>
    /// Summary description for HGStatusTest
    /// </summary>
    [TestClass]
    public class HGStatusTest
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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        static void UpdateRepo(string dir, string expected)
        {
            List<string> resultList;
            Hg.InvokeCommand(dir, "update -C", out resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(expected,resultList[0]);
        }

        [ClassInitialize()]
        public static void ExtractRepositoryFromResource(TestContext testContext)
        {
            Assert.IsTrue(Utilities.ExtractZipResource(testContext.TestDir + "\\HGStatus", typeof(HGStatusTest), "Resources.simple.hg.zip"));
            UpdateRepo(testContext.TestDir + "\\HGStatus", "11 files updated, 0 files merged, 0 files removed, 0 files unresolved");
        }


        [TestMethod]
        public void ReadDirstate()
        {
            Dictionary<string, char> stateMap;
            HGDirstate.ReadDirstate("r:\\VisualHG", out stateMap);
            HGDirstate.ReadDirstate("R:\\SCTest\\NewDialogGB5HG", out stateMap);
        }
    }

}
