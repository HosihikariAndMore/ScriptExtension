using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.ScriptExtension.QuickJS.Types;
using Hosihikari.ScriptExtension.QuickJS.Wrapper;

namespace Hosihikari.ScriptExtension.Hook.QuickJS;

internal class JsNewClass1 : HookBase<JsNewClass1.HookDelegate>
{
    internal unsafe delegate int HookDelegate(JsRuntime* rt, JsClassId id, JsClassDef* def);

    public JsNewClass1()
        : base("JS_NewClass1") { }

    public override unsafe HookDelegate HookedFunc =>
        (rt, id, def) =>
        {
            if (JsRuntimeWrapper.TryGet((nint)rt, out var runtime))
            {
#if DEBUG
                runtime.AddClassInfo(id, def);
#endif
            }
            return Original(rt, id, def);
        };
}
