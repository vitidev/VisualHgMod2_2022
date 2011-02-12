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
        public void PropagateFileRemoved()
        {
            string rName = TestContext.TestDir + "\\HGCommandsTest\\TheApp\\TheApp.sln";
            var fileList = new string[] { rName };
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
        
        [TestMethod]
        public void StatusSubrepo()
        {
            string dir = TestContext.TestDir + "\\HGCommandStatusSubrepoTest";

            Assert.IsTrue(Utilities.ExtractZipResource(dir, typeof(HGStatusTest), "Resources.subrepo.hg.zip"));
            UpdateRepo(dir, "0 files updated, 0 files merged, 0 files removed, 0 files unresolved");

            string[] fileList = new string[] { Path.Combine("subrepo", "subrepofile.cs"), "file.cs" };
            for (int i = 0; i < fileList.Length; i++)
                fileList[i] = Path.Combine(dir, fileList[i]);

            foreach (var file in fileList)
                File.Create(Path.Combine(dir, file));

            Dictionary<string, char> fileStatusDictionary;
            Assert.IsTrue(HGLib.HG.QueryFileStatus(fileList, out fileStatusDictionary));
        }
    }
}
