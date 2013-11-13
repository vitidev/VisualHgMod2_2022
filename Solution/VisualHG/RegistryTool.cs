using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;

namespace VisualHg
{
    public static class RegistryTool
    {
        private static readonly string Prefix = @"SOFTWARE\VisualHg\";
        

        public static void LoadProperties(string keyName, object obj)
        {
            using (var key = OpenKey(keyName))
            {
                if (key == null)
                {
                    return;
                }

                LoadProperties(key, obj);
            }
        }

        public static void StoreProperties(string keyName, object obj)
        {
            using (var key = CreateKey(keyName))
            {
                StoreProperties(key, obj);
            }
        }

        
        private static void LoadProperties(RegistryKey key, object obj)
        {
            foreach (var property in GetProperties(obj))
            {
                SetPropertyValue(key, obj, property);
            }
        }

        private static void StoreProperties(RegistryKey key, object obj)
        {
            foreach (var property in GetProperties(obj))
            {
                var value = property.GetValue(obj);
                key.SetValue(property.Name, property.Converter.ConvertToInvariantString(value));
            }
        }

        private static void SetPropertyValue(RegistryKey key, object obj, PropertyDescriptor property)
        {
            var value = key.GetValue(property.Name, null) as string;

            if (value != null)
            {
                try
                {
                    property.SetValue(obj, property.Converter.ConvertFromInvariantString(value));
                }
                catch { }
            }
        }

        
        private static RegistryKey OpenKey(string keyName)
        {
            if (String.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException("subKey");
            }

            return Registry.CurrentUser.OpenSubKey(Prefix + keyName, RegistryKeyPermissionCheck.ReadSubTree);
        }

        private static RegistryKey CreateKey(string keyName)
        {
            if (String.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException("subKey");
            }

            return Registry.CurrentUser.CreateSubKey(Prefix + keyName);
        }

        private static IEnumerable<PropertyDescriptor> GetProperties(object obj)
        {
            return TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>();
        }
    };
}
