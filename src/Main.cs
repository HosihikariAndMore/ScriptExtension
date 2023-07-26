using Hosihikari.VanillaScript.Hook;
using System.Runtime.CompilerServices;

namespace Hosihikari.VanillaScript;

public static class Main
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Init()
    {
        new EnableScriptingHook().Install();
        new Hook.QuickJS.Eval().Install();
    }
}
