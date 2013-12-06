namespace HgLib.Repository.Commands
{
    public class RemoveFilesHgCommand : HgCommand
    {
        private string[] _fileNames;
        
        public RemoveFilesHgCommand(string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Run(HgRepository repo)
        {
            repo.RemoveFilesInternal(_fileNames);
        }
    }
}
