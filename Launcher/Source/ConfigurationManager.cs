using System;
using System.IO;
using Newtonsoft.Json;

namespace Launcher.Source
{
    public static class ConfigurationManager
    {

        private static readonly string ApplicationStorageDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bdoscientist_Launcher");
        private static readonly string ConfigurationFilePath = Path.Combine(ApplicationStorageDirectoryPath, "settings.json");

        private static readonly JsonSerializer JsonSerializer;

        static ConfigurationManager()
        {
            JsonSerializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            Directory.CreateDirectory(ApplicationStorageDirectoryPath);
        }

        public static Configuration Load()
        {
            if (!File.Exists(ConfigurationFilePath))
                return null;

            using (FileStream fileStream = new FileStream(ConfigurationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                return JsonSerializer.Deserialize<Configuration>(jsonTextReader);
        }

        public static void Save(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            using (FileStream fileStream = new FileStream(ConfigurationFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                JsonSerializer.Serialize(jsonTextWriter, configuration);
        }

    }

}
