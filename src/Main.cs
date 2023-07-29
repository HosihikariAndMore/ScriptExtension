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
        var evalHook = new Hook.QuickJS.Eval();
        evalHook.Install();
        //Native.JsEvalFunc = evalHook.OriginalFunc;
        Assets.Prepare.Init();
    }
}
