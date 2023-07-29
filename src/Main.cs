using Hosihikari.PluginManager;
using Hosihikari.VanillaScript;
using Hosihikari.VanillaScript.Hook;
using Hosihikari.VanillaScript.QuickJS;

[assembly: EntryPoint<Main>]

namespace Hosihikari.VanillaScript;

public class Main : IEntryPoint
{
    public void Initialize(AssemblyPlugin plugin)
    {
        new EnableScriptingHook().Install();
        new Hook.QuickJS.Eval().Install();
        new Hook.QuickJS.FreeContext().Install();
        new Hook.QuickJS.AddIntrinsicBaseObjects().Install();
        Assets.Prepare.Init();
    }
}
