using System;
using HGLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace HGLib.Test
{
    
    /// <summary>
    ///This is a test class for DirectoryWatcherTest and is intended
    ///to contain all DirectoryWatcherTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DirectoryWatcherTest
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

        List<string> watchFileList = new List<string>();

        [TestInitialize()]
        public void InitDirectoryWatcherTest()
        {
            watchFileList.Add(TestContext.TestDir + "\\~WatchChangesA.tmp");
            watchFileList.Add(TestContext.TestDir + "\\~WatchChangesB.tmp");
            watchFileList.Add(TestContext.TestDir + "\\~WatchChangesC.tmp");
            watchFileList.Add(TestContext.TestDir + "\\~WatchChangesD.tmp");
        }

        /// <summary>
        ///A test for DirectoryWatcher Constructor
        ///</summary>
        [TestMethod()]
        public void DirectoryWatcherConstructorTest()
        {
            var watcher = new DirectoryWatcher(TestContext.TestDir);
            watcher.UnsubscribeEvents();

            Assert.AreEqual(watcher._directory, TestContext.TestDir);
            Assert.AreEqual(watcher.LastChangeEvent, DateTime.Today);
        }

        /// <summary>
        /// Watch some change events - create, overwrite, rename and remove
        ///</summary>
        [TestMethod()]
        [DeploymentItem("HGLib.dll")]
        public void WatchSomeChangesTest()
        {
            var watcher = new DirectoryWatcher(TestContext.TestDir);
            
            // watch create new files
            TestContext.WriteLine("watch create new files");
            watchFileList.ForEach(x => File.WriteAllBytes(x, new byte[] { 0, 1, 2 } ));
            System.Threading.Thread.Sleep(150);

            var dirtyFilesMap = watcher.PopDirtyFilesMap();
            Assert.AreEqual(4, dirtyFilesMap.Count);
            watchFileList.ForEach(x => Assert.AreEqual(true, dirtyFilesMap.ContainsKey(x)));
            Assert.IsTrue( new TimeSpan(DateTime.Now.Ticks-watcher.LastChangeEvent.Ticks).TotalMilliseconds < 250);

            // now overwrite the existing files
            TestContext.WriteLine("now overwrite the existing files"); 
            watchFileList.ForEach(x => File.WriteAllBytes(x, new byte[] { 0, 1, 2 }));
            System.Threading.Thread.Sleep(150);
            
            dirtyFilesMap = watcher.PopDirtyFilesMap();
            Assert.AreEqual(4, dirtyFilesMap.Count); 
            watchFileList.ForEach(x => Assert.AreEqual(true, dirtyFilesMap.ContainsKey(x)));
            Assert.IsTrue(new TimeSpan(DateTime.Now.Ticks - watcher.LastChangeEvent.Ticks).TotalMilliseconds < 250);

            // rename files
            TestContext.WriteLine("and at rename files");
            watchFileList.ForEach(x => File.Move(x, x + "r"));
            System.Threading.Thread.Sleep(150);

            dirtyFilesMap = watcher.PopDirtyFilesMap();
            Assert.AreEqual(8, dirtyFilesMap.Count); 
            watchFileList.ForEach(x => Assert.AreEqual(true, dirtyFilesMap.ContainsKey(x)));
            watchFileList.ForEach(x => Assert.AreEqual(true, dirtyFilesMap.ContainsKey(x + "r")));
            Assert.IsTrue(new TimeSpan(DateTime.Now.Ticks - watcher.LastChangeEvent.Ticks).TotalMilliseconds < 250);

            // and at least remove files
            TestContext.WriteLine("and at least remove files"); 
            watchFileList.ForEach(x => File.Delete(x + "r"));
            System.Threading.Thread.Sleep(150);

            dirtyFilesMap = watcher.PopDirtyFilesMap();
            Assert.AreEqual(4, dirtyFilesMap.Count); 
            watchFileList.ForEach(x => Assert.AreEqual(true, dirtyFilesMap.ContainsKey(x + "r")));
            Assert.IsTrue(new TimeSpan(DateTime.Now.Ticks - watcher.LastChangeEvent.Ticks).TotalMilliseconds < 250);

            TestContext.WriteLine("OK"); 
            watcher.UnsubscribeEvents();
        }

        /// <summary>
        ///A test for UnsubscribeEvents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("HGLib.dll")]
        public void UnsubscribeEventsTest()
        {
            var watcher = new DirectoryWatcher(TestContext.TestDir);
            watcher.UnsubscribeEvents();

            File.WriteAllBytes(watchFileList[0], new byte[] { 0, 1, 2 });
            System.Threading.Thread.Sleep(100);
            
            Assert.AreEqual(0, watcher.DirtyFilesCount);
            Assert.AreEqual(DateTime.Today, watcher.LastChangeEvent);
        }

        /// <summary>
        ///A test for PopDirtyFilesMap
        ///</summary>
        [TestMethod()]
        public void PopDirtyFilesMapTest()
        {
            var watcher = new DirectoryWatcher_Accessor(TestContext.TestDir);
            watcher.UnsubscribeEvents();
            watcher._dirtyFilesMap["\\file.h"] = true;
            Assert.AreEqual(1, watcher.DirtyFilesCount);
            var dirtyFilesMap = watcher.PopDirtyFilesMap();
            Assert.AreEqual(0, watcher.DirtyFilesCount);
            Assert.IsTrue(dirtyFilesMap.ContainsKey("\\file.h"));
        }
    }
}
