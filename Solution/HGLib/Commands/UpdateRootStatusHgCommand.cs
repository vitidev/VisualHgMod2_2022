using System.Collections.Generic;

namespace HgLib
{
    public class UpdateRootStatusHgCommand : HgCommand
    {
        private string _directory;
        
        public UpdateRootStatusHgCommand(string directory)
        {
            _directory = directory;
        }

        public void Run(HgRepository repo)
        {
            repo.UpdateRootStatus(_directory);
        }
    }
}
