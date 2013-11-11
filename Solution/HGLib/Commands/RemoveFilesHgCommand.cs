using System.Collections.Generic;

namespace HgLib
{
    public class RemoveFilesHgCommand : HgCommand
    {
        private string[] _fileNames;
        
        public RemoveFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgStatus status, List<string> dirtyFilesList)
        {
            status.EnterFilesRemoved(_fileNames);
            dirtyFilesList.AddRange(_fileNames);
        }
    }
}
