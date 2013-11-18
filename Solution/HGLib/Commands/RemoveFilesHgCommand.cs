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

        public void Run(HgRepository repo)
        {
            repo.RemoveFiles(_fileNames);
        }
    }
}
