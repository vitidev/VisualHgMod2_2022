using System.Collections.Generic;

namespace HgLib
{
    public class UpdateFileStatusHgCommand : HgCommand
    {
        private string[] _fileNames;

        public UpdateFileStatusHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepository repo)
        {
            repo.UpdateFileStatus(_fileNames);
        }
    }
}
