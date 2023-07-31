using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Modules.IO;

internal class Main
{
    public static void Setup(JsContextWrapper ctx)
    {
        var module = ctx.NewModule(Config.ConfigModules.FileIoModuleName);
        FileModule.Setup(ctx, module);
        DirectoryModule.Setup(ctx, module);
        PathModule.Setup(ctx, module);
    }
}
