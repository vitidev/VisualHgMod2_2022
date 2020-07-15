namespace HgLib.Repository.Commands
{
    internal class AddFilesHgCommand : HgCommand
    {
        private readonly string[] _fileNames;

        public AddFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepository repo)
        {   
            repo.AddFilesInternal(_fileNames);
        }
    }
}
