using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HgLib.Test
{
    class HgDirstate
    {
        public static bool ReadDirstate(string root, out Dictionary<string, char> stateMap)
        {
            stateMap = null;

            try
            {
                using (var stream = File.OpenRead(root + "\\.hg\\dirstate"))
                { 
                    var reader = new BinaryReader(stream);

                    stream.Seek(40, SeekOrigin.Begin);

                    stateMap = new Dictionary<string, char>();

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        char state = (char)reader.ReadChar(); 
                        stream.Seek(15, SeekOrigin.Current);
                        string s = reader.ReadString();
                        stateMap[s] = state;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
