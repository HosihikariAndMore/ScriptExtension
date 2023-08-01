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
        new Hook.QuickJS.FreeContext().Install();
        new Hook.QuickJS.FreeRuntime().Install();
        new Hook.QuickJS.NewRuntime2().Install();
        new Hook.QuickJS.JsNewClass1().Install();
        new Hook.QuickJS.AddIntrinsicBaseObjects().Install();
        new Hook.RequestReload().Install();
        new Hook.JsLog.ContextObjectBindPrint().Install();
        Assets.Prepare.Init();
    }
}
