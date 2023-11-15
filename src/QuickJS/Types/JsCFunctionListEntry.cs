using System.Runtime.InteropServices;

namespace Hosihikari.ScriptExtension.QuickJS.Types;

//ref #L965

//typedef struct JSCFunctionListEntry
[StructLayout(LayoutKind.Sequential)]
internal unsafe ref struct JsCFunctionListEntry
{
    //    const char* name;
    public byte* Name;

    //    uint8_t prop_flags;
    public byte PropFlags;

    //    uint8_t def_type;
    [MarshalAs(UnmanagedType.U1)]
    public byte DefType;

    //    int16_t magic;
    public short Magic;
    public CFunctionListEntryUnion U;

    [StructLayout(LayoutKind.Explicit)]
    public ref struct CFunctionListEntryUnion
    {
        //    union {
        public ref struct Func
        {
            //        struct {
            //            uint8_t length; /* XXX: should move outside union */
            public byte length;

            //            uint8_t cproto; /* XXX: should move outside union */
            public byte cproto;

            //            JSCFunctionType cfunc;
            public JsCFunctionType cfunc;
            //        } func;
        }

        [FieldOffset(0)]
        public Func func;

        public ref struct GetSet
        {
            //        struct {
            //            JSCFunctionType get;
            public JsCFunctionType get;

            //            JSCFunctionType set;
            public JsCFunctionType set;
            //        }  getset;
        }

        [FieldOffset(0)]
        public GetSet getset;

        public ref struct Alias
        {
            //        struct {
            //            const char* name;
            public byte* name;

            //            int base;
            public int @base;
            //        } alias;
        }

        [FieldOffset(0)]
        public Alias alias;

        public ref struct PropList
        {
            //        struct {
            //            const struct JSCFunctionListEntry *tab;
            public JsCFunctionListEntry* tab;

            //            int len;
            public int len;
            //        } prop_list;
        }

        [FieldOffset(0)]
        public PropList prop_list;

        //        const char* str;
        //        int32_t i32;
        //        int64_t i64;
        //        double f64;

        [FieldOffset(0)]
        public nint str;

        [FieldOffset(0)]
        public int i32;

        [FieldOffset(0)]
        public long i64;

        [FieldOffset(0)]
        public double f64;
        //    } u;
    }
}
