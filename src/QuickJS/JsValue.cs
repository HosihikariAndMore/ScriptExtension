using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS;

[StructLayout(LayoutKind.Explicit)]
public ref struct JsValue
{
    [StructLayout(LayoutKind.Sequential)]
    internal ref struct JSTagUnion
    {
        private unsafe void* _padding;
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
    internal JSTagUnion _tagdata;
}
