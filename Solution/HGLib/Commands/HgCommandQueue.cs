using System.Collections.Generic;

namespace HgLib
{
    public class HgCommandQueue : Queue<HgCommand>
    {
        public HgCommandQueue DumpCommands() // NOTE: I doubt that this makes sense
        {
            var copy = new HgCommandQueue();

            lock (this)
            {
                while (Count > 0)
                {
	                copy.Enqueue(Dequeue());
                }
            }
            
            return copy;
        }
    }
}
