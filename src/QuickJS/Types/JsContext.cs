using System.Runtime.InteropServices;

namespace Hosihikari.ScriptExtension.QuickJS.Types;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L413
[StructLayout(LayoutKind.Sequential)]
public ref struct JsContext
{
    public JsGCObjectHeader header; /* must come first */
    public unsafe JsRuntime* rt;
    //struct list_head link;

    //uint16_t binary_object_count;
    //int binary_object_size;

    //JSShape* array_shape;   /* initial shape for Array objects */

    //JSValue* class_proto;
    //JSValue function_proto;
    //JSValue function_ctor;
    //JSValue array_ctor;
    //JSValue regexp_ctor;
    //JSValue promise_ctor;
    //JSValue native_error_proto[JS_NATIVE_ERROR_COUNT];
    //JSValue iterator_proto;
    //JSValue async_iterator_proto;
    //JSValue array_proto_values;
    //JSValue throw_type_error;
    //JSValue eval_obj;

    //JSValue global_obj; /* global object */
    //JSValue global_var_obj; /* contains the global let/const definitions */

    //uint64_t random_state;
}
