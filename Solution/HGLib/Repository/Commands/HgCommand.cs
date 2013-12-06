namespace HgLib.Repository.Commands
{
    public interface HgCommand
    {
        void Run(HgRepository repo);
    }
}
