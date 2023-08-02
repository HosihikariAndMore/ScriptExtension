using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

[StructLayout(LayoutKind.Explicit, Size = 16 * 4)]
public ref struct JsPropertyDescriptor
{
    /*
typedef struct JSPropertyDescriptor {
    int flags;
    JSValue value;
    JSValue getter;
    JSValue setter;
} JSPropertyDescriptor;
     */
    [FieldOffset(0)]
    public JsPropertyFlags Flags;

    //ref to js_free_desc(__int64 a1, _QWORD *a2)
    [FieldOffset(16)]
    public JsValue Value;

    [FieldOffset(2 * 16)]
    public JsValue Getter;

    [FieldOffset(3 * 16)]
    public JsValue Setter;
}
