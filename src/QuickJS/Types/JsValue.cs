using Hosihikari.VanillaScript.Hook.JsLog;
using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

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
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct JsValue
{
    //[StructLayout(LayoutKind.Sequential)]
    //internal struct JsTagUnionWithTag
    //{
    //    public unsafe void* ptr;
    //    public JsTag tag;
    //}

    #region JSValueUnion
    [FieldOffset(0)]
    internal ulong uint64;

    [FieldOffset(0)]
    internal int int32;

    [FieldOffset(0)]
    internal double float64;

    [FieldOffset(0)]
    internal unsafe void* ptr;
    #endregion
    [FieldOffset(8)]
    internal JsTag Tag;

    //[FieldOffset(0)]
    //[MarshalAs(UnmanagedType.Struct)]
    //internal JsTagUnionWithTag Data;
}
