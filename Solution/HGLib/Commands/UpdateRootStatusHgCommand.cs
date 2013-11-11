using System.Collections.Generic;

namespace HgLib
{
    public class UpdateRootStatusHgCommand : HgCommand
    {
        private string _root;

        public UpdateRootStatusHgCommand(string root)
        {
            _root = root;
        }

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            status.UpdateFileStatus(_root);
        }
    }
}
