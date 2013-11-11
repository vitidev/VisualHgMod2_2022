using System.Collections.Generic;

namespace HgLib
{
    public class RemoveFilesHgCommand : HgCommand
    {
        private string[] _fileNames;
        
        public RemoveFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.RemoveFiles(_fileNames);
            dirtyFilesList.AddRange(_fileNames);
        }
    }
}
