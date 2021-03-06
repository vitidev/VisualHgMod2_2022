namespace HgLib.Repository.Commands
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

        public void Run(HgRepository repo)
        {
            repo.RenameFilesInternal(_fileNames, _newFileNames);
        }
    }
}
