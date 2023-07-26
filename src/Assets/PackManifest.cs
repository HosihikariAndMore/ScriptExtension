namespace Hosihikari.VanillaScript.Assets;

internal static class PackManifest
{
    private const string Uuid = "2fef88eb-05b4-47eb-97a8-67d3d5e8f31c";
    private const string Data = $$"""
        {
          "format_version": 2,
          "header": {
            "description": "Scripting",
            "name": "Scripting",
            "uuid": "{{Uuid}}",
            "version": [0, 1, 0],
            "min_engine_version": [1, 20, 0]
          },
          "modules": [
            {
              "type": "data",
              "uuid": "628adfcd-dc33-41c0-b9d6-a1a724b03b02",
              "version": [0, 1, 0]
            },
            {
              "type": "script",
              "language": "javascript",
              "uuid": "8c78f4ed-aa7f-46e8-af4f-cb460e0a3de5",
              "version": [0, 1, 0],
              "entry": "scripts/121eiqkr.yel.js"
            }
          ],
          "dependencies": [
            { "module_name": "@minecraft/server-gametest", "version": "1.0.0-beta" },
            { "module_name": "@minecraft/server", "version": "1.4.0-beta" },
            { "module_name": "@minecraft/server-ui", "version": "1.2.0-beta" },
            { "module_name": "@minecraft/server-admin", "version": "1.0.0-beta" },
            { "module_name": "@minecraft/server-net", "version": "1.0.0-beta" }
          ]
        }
        
        """;
}
