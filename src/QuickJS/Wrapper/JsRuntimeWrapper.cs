using Hosihikari.VanillaScript.QuickJS.Types;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hosihikari.NativeInterop.Utils;
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

    public JsClassId NewClass(string name)
    {
        unsafe
        {
            //Native.JS_IsRegisteredClass()
            var id = Native.JS_NewClassID(Runtime);
            fixed (byte* namePtr = StringUtils.StringToManagedUtf8(name))
            {
                var def = new JsClassDef { ClassName = namePtr, };
            }
            //Native.JS_NewClass(Runtime, id,)
            return id;
        }
    }

    private readonly Dictionary<string, JsClassId> _classIdToName = new();
#if DEBUG
    public IEnumerable<string> AllClassName => _classIdToName.Keys;

    public bool TryGetClassId(string className, out JsClassId id)
    {
        return _classIdToName.TryGetValue(className, out id);
    }

    public unsafe void AddClassInfo(JsClassId id, JsClassDef* def)
    {
        if (
            Marshal.PtrToStringUTF8((nint)def->ClassName) is { } className
            && !string.IsNullOrWhiteSpace(className)
        )
        {
            _classIdToName[className] = id;
        }
    }
#endif
}
