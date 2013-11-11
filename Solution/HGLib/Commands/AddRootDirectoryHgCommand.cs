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

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            status.AddRootDirectory(_directory);
        }
    }
}
