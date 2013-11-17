using System.Collections.Generic;

namespace HgLib
{
    public class RenameFilesHgCommand : HgCommand
    {
        private string[] _fileNames;
        private string[] _newFileNames;

        public RenameFilesHgCommand(string[] fileNames, string[] newFileNames)
        {
            _fileNames = fileNames;
            _newFileNames = newFileNames;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            repo.RenameFiles(_fileNames, _newFileNames);
            dirtyFilesList.AddRange(_newFileNames);
        }
    }
}
