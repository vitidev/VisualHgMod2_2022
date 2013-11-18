using System.Linq;
using System.Collections.Generic;

namespace HgLib
{
    public class AddFilesHgCommand : HgCommand
    {
        private string[] _fileNames;

        public AddFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepository repo, List<string> dirtyFilesList)
        {
            foreach (var root in _fileNames.Select(x => HgPath.FindRepositoryRoot(x)).Distinct())
            {
                repo.UpdateRootStatus(root);
            }
            
            repo.AddFiles(_fileNames);
        }
    }
}
