using System.Collections.Generic;

namespace HgLib
{
    public interface HgCommand
    {
        void Run(HgRepository repo);
    }
}
