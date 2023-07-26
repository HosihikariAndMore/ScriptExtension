using Hosihikari.NativeInterop.Hook.ObjectOriented;
using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.Hook;

//force enable scripting
internal class EnableScriptingHook : HookBase<EnableScriptingHook.HookDelegate>
{
    [return: MarshalAs(UnmanagedType.U1)]
    public unsafe delegate bool HookDelegate(void* @this);

    public EnableScriptingHook()
        : base("_ZNK11Experiments8GametestEv") { }

    public override unsafe HookDelegate HookedFunc => _ => true;
}
