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

        static void UpdateRepo(string dir, string result)
        {
            List<string> resultList;
            HG.InvokeCommand(dir, "update", out resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(resultList[0], result);
        }

        [ClassInitialize()]
        public static void ExtractRepositoryFromResource(TestContext testContext)
        {
            Assert.IsTrue(Utilities.ExtractZipResource(testContext.TestDir + "\\HGStatus", typeof(HGStatusTest), "Resources.simple.hg.zip"));
            UpdateRepo(testContext.TestDir + "\\HGStatus", "10 files updated, 0 files merged, 0 files removed, 0 files unresolved");
        }

        [TestMethod]
        public void AsyncPropagateFileRenamed()
        {
            string oName = TestContext.TestDir + "\\HGStatus\\TheApp\\TheApp.sln";
            string nName = TestContext.TestDir + "\\HGStatus\\TheApp\\TheApp1.sln";
            var onList = new string[] { oName };
            var nnList = new string[] { nName };

            HGStatus hgStatus = new HGStatus();
            hgStatus.AddRootDirectory(HG.FindRootDirectory(oName));
            System.Threading.Thread.Sleep(200); // time for rquery repo
            
            // rename file
            {
                File.Move(oName, nName);
                hgStatus.PropagateFileRenamed(onList, nnList);
                System.Threading.Thread.Sleep(400); // time for async task

                SourceControlStatus nStatus = hgStatus.GetFileStatus(nName);
                Assert.AreEqual(nStatus, SourceControlStatus.scsAdded);

                Dictionary<string, char> dictionary;
                HG.QueryFileStatus(onList, out dictionary);
                Assert.AreEqual(dictionary[onList[0]], 'R');
            }

            // and back - here it should be as nothing had happened
            {
                File.Move(nName, oName);
                hgStatus.PropagateFileRenamed(nnList, onList);
                System.Threading.Thread.Sleep(400); // time for async task

                SourceControlStatus oStatus = hgStatus.GetFileStatus(oName);
                Assert.AreEqual(oStatus, SourceControlStatus.scsControlled);

                Dictionary<string, char> dictionary;
                HG.QueryFileStatus(nnList, out dictionary);
                Assert.AreEqual(dictionary.Count, 0);
            }
            
            hgStatus.ClearStatusCache();
        }

        [TestMethod]
        public void AsyncPropagateFileRemoved()
        {
            string rName = TestContext.TestDir + "\\HGStatus\\TheApp\\TheApp.sln";
            var fileList = new string[] { rName };

            HGStatus hgStatus = new HGStatus();
            hgStatus.AddRootDirectory(HG.FindRootDirectory(rName));
            System.Threading.Thread.Sleep(200); // time for rquery repo

            // remove file
            {
                File.Delete(rName);
                hgStatus.PropagateFilesRemoved(fileList);
                System.Threading.Thread.Sleep(400); // time for async task

                SourceControlStatus rStatus = hgStatus.GetFileStatus(rName);
                Assert.AreEqual(rStatus, SourceControlStatus.scsRemoved);
            }

            // revert remove file
            {
                Dictionary<string, char> fileStatusDictionary;
                Assert.IsTrue(HG.Revert(fileList, out fileStatusDictionary));
                Assert.AreEqual(fileStatusDictionary[fileList[0]], 'C');
            }
        }
    }
}
