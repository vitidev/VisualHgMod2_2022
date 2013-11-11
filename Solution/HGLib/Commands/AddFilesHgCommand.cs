using System.Collections.Generic;

namespace HgLib
{
    public class AddFilesHgCommand : HgCommand
    {
        private string[] _fileNames;

        public AddFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            foreach (var fileName in _fileNames)
            {
                status.AddRootDirectory(fileName);
            }
            
            status.AddNotIgnoredFiles(_fileNames);
        }
    }
}
