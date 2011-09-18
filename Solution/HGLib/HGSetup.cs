using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HGLib
{
  /// <summary>
  /// read write THG/mercurial init settings
  /// </summary>
  public class HGSetup
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    /// write data to ini File
    public static void WriteProfileString(string Section, string Key, string Value, string path)
    {
      WritePrivateProfileString(Section, Key, Value, path);
    }

    /// read data value from ini file
    public static string GetProfileString(string Section, string Key, string path)
    {
      StringBuilder temp = new StringBuilder(255);
      int i = GetPrivateProfileString(Section, Key, "", temp,
                                      255, path);
      return temp.ToString();
    }
    
    // find mercurial users ini file
    public static string GetMercurialIniFile()
    {
      // get general setting ini file
      // found one above user/documents. SpecialFolder.UserProfile is not available within MSVS2005
      string iniFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      int index = iniFile.LastIndexOf('\\');
      if(index>0)
        iniFile = iniFile.Substring(0, index);

      return iniFile + "\\mercurial.ini";
    }
    
    // read THG ini setting
    public static string GetTHGSetting(string root, string key)
    {
      // get local setting
      string iniFile = root + "\\.hg\\hgrc";
      string value   = HGSetup.GetProfileString("tortoisehg", key, iniFile);

      // find global setting
      if (value == null || value== string.Empty)
      {
          value = HGSetup.GetProfileString("tortoisehg", key, GetMercurialIniFile());
      }  
      
      return value;  
    }

  }
}