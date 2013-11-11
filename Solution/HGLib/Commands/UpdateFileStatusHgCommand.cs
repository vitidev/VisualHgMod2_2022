using System.Collections.Generic;

namespace HgLib
{
    public class UpdateFileStatusHgCommand : HgCommand
    {
        private string[] _fileNames;

        public UpdateFileStatusHgCommand(string[] _fileNames)
        {
            _fileNames = _fileNames;
        }

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            status.UpdateFileStatus(_fileNames);
        }
    }
}
