using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace HGLib.Test
{
    [TestClass]
    public class HGCommandsTest
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

        static void UpdateRepo(string dir, string expected)
        {
            List<string> resultList;
            HG.InvokeCommand(dir, "update -C", out resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(expected, resultList[0]);
        }

        [ClassInitialize]
        public static void ExtractRepositoryFromResource(TestContext testContext)
        {
            Assert.IsTrue(Utilities.ExtractZipResource(testContext.TestDir + "\\HGCommandsTest", typeof(HGStatusTest), "Resources.simple.hg.zip"));
            UpdateRepo(testContext.TestDir + "\\HGCommandsTest", "11 files updated, 0 files merged, 0 files removed, 0 files unresolved");
        }

        [TestMethod]
        public void AddFile()
        {
            string nName = TestContext.TestDir + "\\HGCommandsTest\\TheApp\\NewFile.txt";
            File.Create(nName);

            Dictionary<string, char> fileStatusDictionary;
            Assert.IsTrue(HG.AddFiles(new string[]{ nName }, out fileStatusDictionary), "add file");
            char status = fileStatusDictionary[nName];
            Assert.AreEqual(status, 'A');
        }

        [TestMethod]
        public void PropagateFileRenamed()
        {
            string oName = TestContext.TestDir + "\\HGCommandsTest\\TheApp\\TheApp.sln";
            string nName = TestContext.TestDir + "\\HGCommandsTest\\TheApp\\TheApp1.sln";
            var onList = new string[] { oName };
            var nnList = new string[] { nName };

            // rename file
            {
                File.Move(oName, nName);

                Dictionary<string, char> fileStatusDictionary;
                Assert.IsTrue(HG.PropagateFileRenamed(onList, nnList, out fileStatusDictionary), "update file renamed");
                char status = fileStatusDictionary[nName];
                Assert.AreEqual(status, 'N'); // internal VisualHG state 'N' for renamed file
                                              // the hg state is A for added file

                Dictionary<string, char> dictionary;
                HG.QueryFileStatus(onList, out dictionary);
                Assert.AreEqual(dictionary[onList[0]], 'R');

            }

            // and back - here it should be as nothing had happened
            {
                File.Move(nName, oName);

                Dictionary<string, char> fileStatusDictionary;
                Assert.IsTrue(HG.PropagateFileRenamed(nnList, onList, out fileStatusDictionary), "update file renamed to prev name");
                char status = fileStatusDictionary[oName];
                Assert.AreEqual(status, 'C');

                Dictionary<string, char> dictionary;
                HG.QueryFileStatus(nnList, out dictionary);
                Assert.AreEqual(dictionary.Count, 0);
            }
        }

        [TestMethod]
        public void PropagateFileRemoved()
        {
            string rName = TestContext.TestDir + "\\HGCommandsTest\\TheApp\\TheApp.sln";
            var fileList = new string[] { rName };

            // remove file
            {
                File.Delete(rName);

                Dictionary<string, char> fileStatusDictionary;
                Assert.IsTrue(HG.PropagateFileRemoved(fileList, out fileStatusDictionary));
                char status = fileStatusDictionary[rName];
                Assert.AreEqual(status, 'R');
            }

            // revert remove file
            {
                Dictionary<string, char> fileStatusDictionary;
                Assert.IsTrue(HG.Revert(fileList, out fileStatusDictionary));
                char status = fileStatusDictionary[rName];
                Assert.AreEqual(status, 'C');
            }
        }
        [TestMethod]
        public void AddFiles()
        {
            string dir = TestContext.TestDir + "\\HGCommandAddFilesTest";

            Assert.IsTrue(Utilities.ExtractZipResource(dir, typeof(HGStatusTest), "Resources.simple.hg.zip"));
            UpdateRepo(dir, "11 files updated, 0 files merged, 0 files removed, 0 files unresolved");

            // create some new files on disk
            string[] fileList = new string[] { dir + "\\IgnoreFile.cs", dir + "\\NotIgnoredFile.cs" };
            foreach (var file in fileList)
                File.Create(file);

            Dictionary<string, char> fileStatusDictionary;
            Assert.IsTrue(HGLib.HG.AddFilesNotIgnored(fileList, out fileStatusDictionary));

            Assert.IsFalse(fileStatusDictionary.ContainsKey(fileList[0]));
            Assert.AreEqual(fileStatusDictionary[fileList[1]], 'A');

            HGLib.HG.QueryFileStatus(fileList, out fileStatusDictionary);
            Assert.AreEqual(fileStatusDictionary[fileList[0]], 'I');
            Assert.AreEqual(fileStatusDictionary[fileList[1]], 'A');
        }
        
    }
}
