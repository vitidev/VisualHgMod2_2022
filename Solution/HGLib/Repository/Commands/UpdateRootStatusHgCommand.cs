namespace HgLib.Repository.Commands
{
    public class UpdateRootStatusHgCommand : HgCommand
    {
        private readonly string _directory;
        
        public UpdateRootStatusHgCommand(string directory)
        {
            _directory = directory;
        }

        public void Run(HgRepository repo)
        {
            repo.UpdateRootStatusInternal(_directory);
        }
    }
}
