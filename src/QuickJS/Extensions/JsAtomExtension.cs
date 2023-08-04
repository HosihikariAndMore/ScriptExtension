using System.Runtime.CompilerServices;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

public static class JsAtomExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string ToString(this JsAtom atom, JsContext* ctx)
    {
        return Native.JS_AtomToCString(ctx, atom);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToString(this JsAtom atom, JsContextWrapper ctx)
    {
        unsafe
        {
            return Native.JS_AtomToCString(ctx.Context, atom);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryGetIndex(this JsAtom atom, JsContextWrapper ctx, out uint index) =>
        TryGetIndex(atom, ctx.Context, out index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryGetIndex(this JsAtom atom, JsContext* ctx, out uint index)
    {
        return Native.JS_AtomIsArrayIndex(ctx, out index, atom);
    }

    public static unsafe void UnsafeRemoveRefCount(this JsAtom atom, JsContext* ctx)
    {
        if (atom != default)
        {
            Native.JS_FreeAtom(ctx, atom);
        }
    }
}
