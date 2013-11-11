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

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.UpdateRootStatus(_root);
        }
    }
}
