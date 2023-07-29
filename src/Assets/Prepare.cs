using System.Text.Json.Nodes;
using Hosihikari.Minecraft.Extension.PackHelper;

namespace Hosihikari.VanillaScript.Assets;

internal static class Prepare
{
    public static string EntryPointJsName = "entry_point_" + Guid.NewGuid().ToString("N") + ".js";
    public const string SuccessScriptContent = $"console.log('{nameof(VanillaScript)} Loaded !');";
    public const string FailedScriptContent =
        $"console.error('{nameof(VanillaScript)} Load Failed !');";

    internal static void Init()
    {
        var pack = Path.GetFullPath(Path.Combine("config", nameof(VanillaScript), "pack"));
        var manifest = Path.Combine(pack, "manifest.json");
        var scripts = Path.Combine(pack, "scripts");
        if (!Directory.Exists(pack))
            Directory.CreateDirectory(pack);
        if (Directory.Exists(scripts))
            Directory.Delete(scripts, true);
        Directory.CreateDirectory(scripts);
        //if (!File.Exists(manifest))
        File.WriteAllText(manifest, PackManifest.Data);
        File.WriteAllText(Path.Combine(scripts, EntryPointJsName), FailedScriptContent);
        PackHelper.AddPack(
            PackType.BehaviorPack,
            pack,
            new(Guid.Parse(PackManifest.Uuid), (0, 1, 0))
        );
        FixConfig();
    }

    private static void FixConfig()
    {
        var configFile = Path.GetFullPath(Path.Combine("config", "default", "permissions.json"));
        if (File.Exists(configFile))
        {
            var json = JsonNode.Parse(File.ReadAllText(configFile));
            if (json is not null)
            {
                json["allowed_modules"] = new JsonArray
                {
                    "@minecraft/server-gametest",
                    "@minecraft/server",
                    "@minecraft/server-ui",
                    "@minecraft/server-admin",
                    "@minecraft/server-editor",
                    "@minecraft/server-net"
                };
                File.WriteAllText(configFile, json.ToString());
            }
        }
    }
}
