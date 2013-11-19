using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public class UpdateRootStatusHgCommand : HgCommand
    {
        private string _directory;
        
        public UpdateRootStatusHgCommand(string directory)
        {
            _directory = directory;
        }

        public void Run(HgRepositoryBase repo)
        {
            repo.UpdateRootStatus(_directory);
        }
    }
}
