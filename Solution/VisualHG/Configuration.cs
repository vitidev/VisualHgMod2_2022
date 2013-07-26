using System;
using System.Text;

namespace VisualHg
{
    /// <summary>
    /// visualhg configuration properties container
    /// </summary>
    public class Configuration
    {
        public bool   _autoAddFiles = true;
        public bool   _autoActivatePlugin = true;
        public bool   _enableContextSearch = true;
        public bool   _observeOutOfStudioFileChanges = true;

        public string _externalDiffToolCommandMask = string.Empty; 

        public bool AutoActivatePlugin
        {
            get { return _autoActivatePlugin; }
            set { _autoActivatePlugin = value; }
        }

        public bool AutoAddFiles
        {
            get { return _autoAddFiles; }
            set { _autoAddFiles = value; }
        }

        public string ExternalDiffToolCommandMask
        {
            get { return _externalDiffToolCommandMask; }
            set { _externalDiffToolCommandMask = value; }
        }

        public bool EnableContextSearch
        {
            get { return _enableContextSearch; }
            set { _enableContextSearch = value; }
        }

        public bool ObserveOutOfStudioFileChanges
        {
            get { return _observeOutOfStudioFileChanges; }
            set { _observeOutOfStudioFileChanges = value; }
        }
        
        /// <summary>
        /// global accessible settings object
        /// </summary>
        static Configuration _global = null;
        public static Configuration Global
        {
            get {
                if (_global == null)
                {
                    _global = LoadConfiguration();
                }
                return _global;
              }

            set {
                _global = value;
              }
        }
        
        /// <summary>
        /// read properties from regestry
        /// </summary>
        /// <returns></returns>
        static public Configuration LoadConfiguration()
        {
            Configuration configuration = new Configuration();
            RegestryTool.LoadProperties("Configuration", configuration);
            return configuration;
        }
        
        /// <summary>
        /// store properties to regestry
        /// </summary>
        public void StoreConfiguration()
        {
            RegestryTool.StoreProperties("Configuration", this);
        }

    }
}
