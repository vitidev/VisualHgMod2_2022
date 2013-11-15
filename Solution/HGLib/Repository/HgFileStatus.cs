namespace HgLib
{
    public enum HgFileStatus
    {
        Uncontrolled = 0x01,
        Added = 0x02,
        Clean = 0x04,
        Modified = 0x08,
        Removed = 0x10,
        Renamed = 0x20,
        Copied = 0x40,
        Ignored = 0x80,
        Missing = 0x100,

        Different = Added | Modified | Removed | Renamed | Copied,
        Comparable = Modified | Removed | Missing | Renamed | Copied,
        Controlled = Clean | Modified | Removed | Missing | Renamed | Copied,
    };
}
