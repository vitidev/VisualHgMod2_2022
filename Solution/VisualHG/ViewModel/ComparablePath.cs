using System;
using System.Linq;

namespace VisualHg.ViewModel
{
    public class ComparablePath : IComparable, IComparable<ComparablePath>
    {
        public string Value { get; set; }


        public ComparablePath()
        {
        }

        public ComparablePath(string value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return Value;
        }


        public int CompareTo(object obj)
        {
            return CompareTo((ComparablePath)obj);
        }

        public int CompareTo(ComparablePath other)
        {
            var result = GetPathDepth(Value).CompareTo(GetPathDepth(other.Value));

            if (result == 0)
            {
                result = Value.CompareTo(other.Value);
            }

            return result;
        }

        private static int GetPathDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            return path.Count(x => x == '\\');
        }


        public static implicit operator ComparablePath(string path)
        {
            return new ComparablePath(path);
        }

        public static explicit operator string(ComparablePath path)
        {
            return path.Value;
        }
    }
}