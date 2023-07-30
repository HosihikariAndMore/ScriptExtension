using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hosihikari.VanillaScript;

internal static class Config
{
    internal class ConfigData
    {
        public bool EnableEval { get; set; } = false;

        public bool EnableLogger { get; set; } = true;

        public ConfigModules BuildInModules { get; set; } = new();
    }

    internal class ConfigModules
    {
        public const string FileIoModuleName = "@hosihikari/io";

        [JsonPropertyName(FileIoModuleName)]
        public bool FileIo { get; set; } = true;
    }

    internal static ConfigData Data { get; private set; }

    private static FileInfo ConfigFile
    {
        get
        {
            var configDirectory = new DirectoryInfo(
                Path.GetFullPath(Path.Combine("config", nameof(VanillaScript)))
            );
            if (!configDirectory.Exists)
                configDirectory.Create();
            var configFile = new FileInfo(Path.Combine(configDirectory.FullName, "config.json"));
            return configFile;
        }
    }

    static Config()
    {
        var configFile = ConfigFile;
        ConfigData configData = configFile.Exists
            ? JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(configFile.FullName)) ?? new()
            : new();
        File.WriteAllText(
            configFile.FullName,
            JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true })
        );
        Data = configData;
    }

    //todo : reload config when reload script
    internal static void Reload()
    {
        var configFile = ConfigFile;
        if (configFile.Exists)
        {
            if (
                JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(configFile.FullName)) is
                { } configData
            )
            {
                Data = configData;
            }
        }
    }
}
