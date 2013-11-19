using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public class UpdateFileStatusHgCommand : HgCommand
    {
        private string[] _fileNames;

        public UpdateFileStatusHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepositoryBase repo)
        {
            repo.UpdateFileStatus(_fileNames);
        }
    }
}
