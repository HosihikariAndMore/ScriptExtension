using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

/*
typedef struct JSPropertyEnum {
    JS_BOOL is_enumerable;
    JSAtom atom;
} JSPropertyEnum;
 */
[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct JsPropertyEnum
{
    [MarshalAs(UnmanagedType.I4)]
    public bool IsEnumerable;

    [MarshalAs(UnmanagedType.U4)]
    public JsAtom Atom;
}
