using System.Collections.Generic;

namespace HgLib
{
    public class UpdateRootDirectoryAdded : HgCommand
    {
        private string _directory;
        
        public UpdateRootDirectoryAdded(string directory)
        {
            _directory = directory;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.AddRootDirectory(_directory);
        }
    }
}
