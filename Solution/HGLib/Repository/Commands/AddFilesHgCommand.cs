using System.Linq;
using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public class AddFilesHgCommand : HgCommand
    {
        private string[] _fileNames;

        public AddFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepositoryBase repo)
        {   
            repo.AddFiles(_fileNames);
        }
    }
}
