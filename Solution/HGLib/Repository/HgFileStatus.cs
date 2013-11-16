namespace HgLib
{
    public enum HgFileStatus
    {
        None = 0x00,
        Modified = 0x01,
        Added = 0x02,
        Removed = 0x04,
        Clean = 0x08,
        Missing = 0x10,
        NotTracked = 0x20,
        Ignored = 0x40,

        Renamed = 0x80,
        Copied = 0x100,

        Tracked = Modified | Removed | Clean | Missing | Renamed | Copied,
        Different = Modified | Added | Removed | Missing | Renamed | Copied,
        Comparable = Modified | Renamed | Copied,
        Deleted = Removed | Missing,
    };
}