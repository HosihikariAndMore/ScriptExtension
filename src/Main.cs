using Hosihikari.PluginManager;
using Hosihikari.VanillaScript;
using Hosihikari.VanillaScript.Hook;

[assembly: EntryPoint<Main>]

namespace Hosihikari.VanillaScript;

public class Main : IEntryPoint
{
    public void Initialize(AssemblyPlugin plugin)
    {
        new EnableScriptingHook().Install();
        new Hook.QuickJS.Eval().Install();
        Assets.Prepare.Init();
    }
}
