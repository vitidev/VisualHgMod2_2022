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

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            status.EnterFileRenamed(_oldFileNames, _newFileNames);
            dirtyFilesList.AddRange(_newFileNames);
        }
    }
}
