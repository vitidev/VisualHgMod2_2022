using System.Collections.Generic;

namespace HgLib
{
    public class AddRootDirectoryHgCommand : HgCommand
    {
        private string _directory;
        
        public AddRootDirectoryHgCommand(string directory)
        {
            _directory = directory;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.AddRootDirectory(_directory);
        }
    }
}
