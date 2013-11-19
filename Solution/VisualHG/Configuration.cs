using System;
using System.IO;

namespace VisualHg
{
    public class Configuration
    {
        public bool AutoActivatePlugin { get; set; }

        public bool AutoAddFiles { get; set; }

        public bool SearchIncludingChildren { get; set; }

        public string DiffToolPath { get; set; }

        public string DiffToolArguments { get; set; }


        public Configuration()
        {
            AutoActivatePlugin = true;
            AutoAddFiles = true;
            SearchIncludingChildren = true;
            DiffToolPath = "";
            DiffToolArguments = "";
        }

        
        private static Configuration _global;

        public static Configuration Global
        {
            get { return _global ?? (_global = LoadConfiguration()); }
            set
            {
                _global = value;
                SaveConfiguration(_global);
            }
        }


        private static string configurationPath = Path.Combine
               (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"VisualHg2\Configuration.xml");

        private static Configuration LoadConfiguration()
        {
            return Serializer.Deserialize<Configuration>(configurationPath) ?? new Configuration();
        }

        private static void SaveConfiguration(Configuration configuration)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configurationPath));

            Serializer.Serialize(configurationPath, configuration);
        }

    }
}