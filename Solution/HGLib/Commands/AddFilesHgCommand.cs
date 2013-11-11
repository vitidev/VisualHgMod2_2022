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

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            foreach (var fileName in _fileNames)
            {
                repo.AddRootDirectory(fileName);
            }
            
            repo.AddFiles(_fileNames);
        }
    }
}
