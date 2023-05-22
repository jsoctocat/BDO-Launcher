using System;
using System.IO;
using Newtonsoft.Json;

namespace Launcher.Source
{

    public static class ConfigurationManager
    {

        private static readonly string _applicationStorageDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bdoscientist_Launcher");
        private static readonly string _configurationFilePath = Path.Combine(_applicationStorageDirectoryPath, "settings.json");

        private static JsonSerializer _jsonSerializer;

        static ConfigurationManager()
        {
            _jsonSerializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            Directory.CreateDirectory(_applicationStorageDirectoryPath);
        }

        public static Configuration Load()
        {
            if (!File.Exists(_configurationFilePath))
                return null;

            using (FileStream fileStream = new FileStream(_configurationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                return _jsonSerializer.Deserialize<Configuration>(jsonTextReader);
        }

        public static void Save(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            using (FileStream fileStream = new FileStream(_configurationFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                _jsonSerializer.Serialize(jsonTextWriter, configuration);
        }

    }

}
