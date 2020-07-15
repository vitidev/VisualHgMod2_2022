namespace HgLib.Repository.Commands
{
    public class RenameFilesHgCommand : HgCommand
    {
        private readonly string[] _fileNames;
        private readonly string[] _newFileNames;

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
