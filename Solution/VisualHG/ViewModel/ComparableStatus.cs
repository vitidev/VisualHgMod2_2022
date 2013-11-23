using System;
using HgLib;

namespace VisualHg.ViewModel
{
    public class ComparableStatus : IComparable, IComparable<ComparableStatus>
    {
        public HgFileStatus Value { get; set; }

        
        public ComparableStatus() { }

        public ComparableStatus(HgFileStatus value)
        {
            Value = value;
        }
        

        public override string ToString()
        {
            return Value.ToString();
        }


        public int CompareTo(object obj)
        {
            return CompareTo((ComparableStatus)obj);
        }

        public int CompareTo(ComparableStatus other)
        {
            return GetStatusPriority(Value).CompareTo(GetStatusPriority(other.Value));
        }

        private static int GetStatusPriority(HgFileStatus status)
        {
            switch (status)
            {
                case HgFileStatus.Modified:
                    return 0;
                case HgFileStatus.Copied:
                    return 2;
                case HgFileStatus.Renamed:
                    return 1;
                case HgFileStatus.Added:
                    return 3;
                case HgFileStatus.Removed:
                    return 4;
                case HgFileStatus.Missing:
                    return 5;
                case HgFileStatus.NotTracked:
                    return 6;
                case HgFileStatus.Ignored:
                    return 7;
                case HgFileStatus.Clean:
                    return 8;
                default:
                    return 9;
            }
        }


        public static implicit operator ComparableStatus(HgFileStatus status)
        {
            return new ComparableStatus(status);
        }

        public static explicit operator HgFileStatus(ComparableStatus status)
        {
            return status.Value;
        }
    }
}
