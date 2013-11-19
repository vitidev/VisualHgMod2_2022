using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public class RemoveFilesHgCommand : HgCommand
    {
        private string[] _fileNames;
        
        public RemoveFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepositoryBase repo)
        {
            repo.RemoveFiles(_fileNames);
        }
    }
}
