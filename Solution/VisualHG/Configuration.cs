namespace VisualHg
{
    public class Configuration
    {
        private static Configuration _global;

        public static Configuration Global
        {
            get { return _global ?? (_global = LoadConfiguration()); }
            set { _global = value; }
        }


        public bool AutoActivatePlugin { get; set; }

        public bool AutoAddFiles { get; set; }

        public bool EnableContextSearch { get; set; }

        public bool ObserveOutOfStudioFileChanges { get; set; }

        public string ExternalDiffToolCommandMask { get; set; }


        public Configuration()
        {
            AutoActivatePlugin = true;
            AutoAddFiles = true;
            EnableContextSearch = true;
            ObserveOutOfStudioFileChanges = true;
            ExternalDiffToolCommandMask = "";
        }

        
        public void StoreConfiguration()
        {
            RegistryTool.StoreProperties("Configuration", this);
        }
        
        public static Configuration LoadConfiguration()
        {
            var configuration = new Configuration();
            
            RegistryTool.LoadProperties("Configuration", configuration);
            
            return configuration;
        }
    }
}
