using System.Collections.Generic;

namespace HgLib.Repository.Commands
{
    public class HgCommandQueue
    {
        private Queue<HgCommand> items;

        public object SyncRoot { get; private set; }

        public HgCommandQueue()
        {
            items = new Queue<HgCommand>();
            SyncRoot = new object();
        }

        public void Enqueue(HgCommand command)
        {
            lock (SyncRoot)
            {
                items.Enqueue(command);
            }
        }

        public HgCommand[] Dump()
        {
            var commands = new List<HgCommand>();

            lock (SyncRoot)
            {
                while (items.Count > 0)
                {
	                commands.Add(items.Dequeue());
                }
            }
            
            return commands.ToArray();
        }
    }
}
