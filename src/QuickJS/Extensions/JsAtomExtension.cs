using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

public static class JsAtomExtension
{
    public static unsafe string ToString(this JsAtom atom, JsContext* ctx)
    {
        return Native.JS_AtomToCString(ctx, atom);
    }

    public static string ToString(this JsAtom atom, JsContextWrapper ctx)
    {
        unsafe
        {
            return Native.JS_AtomToCString(ctx.Context, atom);
        }
    }

    public static unsafe void UnsafeRemoveRefCount(this JsAtom atom, JsContext* ctx)
    {
        if (atom != default)
        {
            Native.JS_FreeAtom(ctx, atom);
        }
    }
}
