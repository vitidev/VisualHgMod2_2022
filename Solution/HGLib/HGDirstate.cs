using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HGLib
{
    // only for test purpose
    class HGDirstate
    {
        public static bool ReadDirstate(string root, out Dictionary<string, char> stateMap)
        {
            stateMap = null;

            try
            {
                FileStream fs = new FileStream(root + "\\.hg\\dirstate",
                                                FileMode.Open,
                                                FileAccess.Read,
                                                FileShare.Read);

                BinaryReader reader = new BinaryReader(fs);

                reader.BaseStream.Seek(40, SeekOrigin.Begin);

                stateMap = new Dictionary<string, char>();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    char state = (char)reader.ReadChar(); 
                    reader.BaseStream.Seek(15, SeekOrigin.Current);
                    string s = reader.ReadString();
                    stateMap[s] = state;
                }
                return true;
            }
            catch
            {
            }
            
            return false;
        }
    }
}
