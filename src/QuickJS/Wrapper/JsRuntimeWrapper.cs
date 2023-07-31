using Hosihikari.VanillaScript.QuickJS.Types;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.Loader;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public class JsRuntimeWrapper
{
    public unsafe JsRuntime* Runtime { get; }
    private readonly List<GCHandle> _savedObject = new();

    internal void Pin(object obj)
    {
        _savedObject.Add(GCHandle.Alloc(obj));
    }

    public static unsafe implicit operator JsRuntimeWrapper(JsRuntime* rt)
    {
        return FetchOrCreate(rt);
    }

    private unsafe JsRuntimeWrapper(JsRuntime* rt)
    {
        Runtime = rt;
        Manager.SetupRuntime(this);
    }

    public static bool TryGet(nint ctxPtr, [NotNullWhen(true)] out JsRuntimeWrapper? ctx)
    {
        unsafe
        {
            if (
                Manager.LoadedScriptsRuntime.FirstOrDefault(x => x.Runtime == ctxPtr.ToPointer()) is
                { } oldCtx
            )
            {
                ctx = oldCtx;
                return true;
            }

            ctx = null;
            return false;
        }
    }

    public static unsafe JsRuntimeWrapper FetchOrCreate(JsRuntime* ctx)
    {
        if (Manager.LoadedScriptsRuntime.FirstOrDefault(x => x.Runtime == ctx) is { } oldCtx)
        {
            return oldCtx;
        }
        var newInstance = new JsRuntimeWrapper(ctx);
        Manager.LoadedScriptsRuntime.Add(newInstance);
        return newInstance;
    }

    internal void Free()
    {
        foreach (var pinedItem in _savedObject)
        {
            pinedItem.Free();
        }
    }
}
