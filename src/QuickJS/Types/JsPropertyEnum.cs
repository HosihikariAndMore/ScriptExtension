using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

/*
typedef struct JSPropertyEnum {
    JS_BOOL is_enumerable;
    JSAtom atom;
} JSPropertyEnum;
 */
[StructLayout(LayoutKind.Sequential)]
public struct JsPropertyEnum
{
    [MarshalAs(UnmanagedType.I4)]
    public bool IsEnumerable;

    [MarshalAs(UnmanagedType.U4)]
    public uint Atom;
}
