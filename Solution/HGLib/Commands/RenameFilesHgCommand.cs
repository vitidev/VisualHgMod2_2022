using System.Collections.Generic;

namespace HgLib
{
    public class RenameFilesHgCommand : HgCommand
    {
        private string[] _oldFileNames;
        private string[] _newFileNames;

        public RenameFilesHgCommand(string[] oldFileNames, string[] newFileNames)
        {
            _oldFileNames = oldFileNames;
            _newFileNames = newFileNames;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.RenameFiles(_oldFileNames, _newFileNames);
            dirtyFilesList.AddRange(_newFileNames);
        }
    }
}
