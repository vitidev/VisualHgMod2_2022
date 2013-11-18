using System.Linq;
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
            repo.AddFiles(_fileNames);
        }
    }
}
