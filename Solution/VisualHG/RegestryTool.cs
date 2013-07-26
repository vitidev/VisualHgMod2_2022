using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.Win32;

namespace VisualHg
{
    /// <summary>
    /// registry read/write helper tool
    /// </summary>
    public class RegestryTool
    {
        /// <summary>
        // Opens the requested key or returns null
        /// </summary>
        /// <param name="subKey"></param>
        /// <returns></returns>
        static RegistryKey OpenRegKey(string subKey)
        {
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");

            return Registry.CurrentUser.OpenSubKey("SOFTWARE\\VisualHg\\" + subKey, RegistryKeyPermissionCheck.ReadSubTree);
        }

        /// <summary>
        /// Opens or creates the requested key
        /// </summary>
        static RegistryKey OpenCreateKey(string subKey)
        {
            if (string.IsNullOrEmpty(subKey))
                throw new ArgumentNullException("subKey");

            return Registry.CurrentUser.CreateSubKey("SOFTWARE\\VisualHg\\" + subKey);
        }

        /// <summary>
        /// load object properties from registry
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="o"></param>
        static public void LoadProperties(string keyName, Object o)
        {
            using (RegistryKey reg = OpenRegKey(keyName))
            {
                if (reg != null)
                {
                    PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(o);
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        string value = reg.GetValue(pd.Name, null) as string;

                        if (value != null)
                        {
                            try
                            {
                                pd.SetValue(o, pd.Converter.ConvertFromInvariantString(value));
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// store object properties to registry
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="o"></param>
        static public void StoreProperties(string keyName, Object o)
        {
            using (RegistryKey reg = OpenCreateKey(keyName))
            {
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(o);
                foreach (PropertyDescriptor pd in pdc)
                {
                    object value = pd.GetValue(o);
                    reg.SetValue(pd.Name, pd.Converter.ConvertToInvariantString(value));
                }
            }
        }
    };
}
