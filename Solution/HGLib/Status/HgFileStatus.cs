namespace HgLib
{
    public enum HgFileStatus
    {
        Uncontrolled = 0x001,
        Clean        = 0x002,
        Modified     = 0x004,
        Added        = 0x008,
        Removed      = 0x010,
        Renamed      = 0x020,
        Copied       = 0x040,
        Ignored      = 0x080,
        Missing      = 0x100,
    };
}
