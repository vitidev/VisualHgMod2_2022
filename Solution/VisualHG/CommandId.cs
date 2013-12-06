namespace VisualHg
{
    public static class CommandId
    {
        public const int PendingChanges      = 0x100;
     
        public const int Commit              = 0x101;
        public const int Status              = 0x102;
        public const int Workbench           = 0x103;
        public const int Synchronize         = 0x104;
        public const int Update              = 0x105;
                                             
        public const int Add                 = 0x106;
        public const int CommitSelected      = 0x107;
        public const int Diff                = 0x108;
        public const int Revert              = 0x109;
        public const int History             = 0x110;

        public const int CreateRepository    = 0x111;
        public const int Settings            = 0x112;
        public const int Shelve              = 0x113;
    }
}