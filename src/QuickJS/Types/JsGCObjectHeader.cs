using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

/// <summary>
///header for GC objects. GC objects are C data structures with a reference count that can reference other GC objects.JS Objects are a particular type of GC object.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public ref struct JsGCObjectHeader
{
    public int ref_count; /* must come first, 32-bit */

    //JSGCObjectTypeEnum gc_obj_type : 4;
    //byte mark : 4;
    public byte gc_obj_type_and_mark;
    public byte dummy1; /* not used by the GC */
    public short dummy2; /* not used by the GC */
    public ListHead link;

    public struct ListHead
    {
        public unsafe ListHead* prev;
        public unsafe ListHead* next;
    };
    //public enum JSGCObjectTypeEnum
    //{
    //    JS_GC_OBJ_TYPE_JS_OBJECT,
    //    JS_GC_OBJ_TYPE_FUNCTION_BYTECODE,
    //    JS_GC_OBJ_TYPE_SHAPE,
    //    JS_GC_OBJ_TYPE_VAR_REF,
    //    JS_GC_OBJ_TYPE_ASYNC_FUNCTION,
    //    JS_GC_OBJ_TYPE_JS_CONTEXT,
    //}
}