using System;
using System.Text;

namespace VisualHG
{
    /// <summary>
    /// visualhg configuration properties container
    /// </summary>
    public class Configuration
    {
        public bool _autoAddFiles = true;
        public bool _autoActivatePlugin = true;

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
