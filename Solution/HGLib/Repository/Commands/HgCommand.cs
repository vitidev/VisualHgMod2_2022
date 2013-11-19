using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public interface HgCommand
    {
        void Run(HgRepositoryBase repo);
    }
}
