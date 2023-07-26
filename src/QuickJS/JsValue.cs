using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L197
/*
 typedef union JSValueUnion {
    int32_t int32;
    double float64;
    void *ptr;
} JSValueUnion;

typedef struct JSValue {
    JSValueUnion u;
    int64_t tag;
} JSValue;
 */
[StructLayout(LayoutKind.Explicit)]
public ref struct JsValue
{
    [StructLayout(LayoutKind.Sequential)]
    internal ref struct JsTagUnionWithTag
    {
        public int int32;
        public double float64;
        public JsTag tag;
    }

    [FieldOffset(0)]
    internal ulong uint64;

    [FieldOffset(0)]
    internal int int32;

    [FieldOffset(0)]
    internal double float64;

    [FieldOffset(0)]
    internal unsafe void* ptr;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.Struct)]
    internal JsTagUnionWithTag Data;
}
