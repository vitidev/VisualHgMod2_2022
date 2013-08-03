using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace HgLib.Test
{
    /// <summary>
    /// Summary description for HgStatusTest
    /// </summary>
    [TestClass]
    public class HgStatusTest
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
            var resultList = Hg.Update(dir);
            
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(expected,resultList[0]);
        }

        [ClassInitialize()]
        public static void ExtractRepositoryFromResource(TestContext testContext)
        {
            Assert.IsTrue(Utilities.ExtractZipResource(testContext.TestDir + "\\HgStatus", typeof(HgStatusTest), "Resources.simple.hg.zip"));
            UpdateRepo(testContext.TestDir + "\\HgStatus", "11 files updated, 0 files merged, 0 files removed, 0 files unresolved");
        }


        [TestMethod]
        public void ReadDirstate()
        {
            Dictionary<string, char> stateMap;
            HgDirstate.ReadDirstate("r:\\VisualHg", out stateMap);
            HgDirstate.ReadDirstate("R:\\SCTest\\NewDialogGB5Hg", out stateMap);
        }
    }

}
