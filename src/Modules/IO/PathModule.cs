using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Modules.IO;

internal class PathModule
{
    public static void Setup(JsContextWrapper ctx, JsModuleDefWrapper module)
    {
        module.AddExportValue("currentDirectory", _ => ctx.NewString(Environment.CurrentDirectory));
    }
}
