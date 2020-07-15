using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VisualHg
{
    public class Serializer
    {
        public static bool Serialize<T>(string path, T instance) where T : class
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                return false;
            }

            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineHandling = NewLineHandling.Entitize,
                };

                using (var xmlWriter = XmlWriter.Create(path, settings))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(xmlWriter, instance);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static T Deserialize<T>(string path) where T : class
        {
            if (string.IsNullOrEmpty(path))
            {
                return default;
            }

            if (!File.Exists(path))
            {
                return default;
            }

            try
            {
                using (var textReader = new StreamReader(path))
                {
                    var settings = new XmlReaderSettings {IgnoreWhitespace = true};
                    var reader = XmlReader.Create(textReader, settings);
                    var xmlSerializer = new XmlSerializer(typeof(T));

                    if (xmlSerializer.CanDeserialize(reader))
                    {
                        return xmlSerializer.Deserialize(reader) as T;
                    }
                }
            }
            catch
            {
                return default;
            }

            return default;
        }
    }
}