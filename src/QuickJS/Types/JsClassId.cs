using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.QuickJS.Types;

[StructLayout(LayoutKind.Sequential, Size = sizeof(uint))]
public struct JsClassId
{
    public uint Id;

    public static bool operator ==(JsClassId left, JsClassId right) => left.Equals(right);

    public static bool operator !=(JsClassId left, JsClassId right) => !(left == right);

    public override bool Equals(object? obj) => obj is JsClassId other && Equals(other);

    public bool Equals(JsClassId other)
    {
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (int)Id;
    }

    public enum BuildIn
    {
        JsClassObject = 1, /* must be first */
        JsClassArray, /* u.array       | length */
        JsClassError,
        JsClassNumber, /* u.object_data */
        JsClassString, /* u.object_data */
        JsClassBoolean, /* u.object_data */
        JsClassSymbol, /* u.object_data */
        JsClassArguments, /* u.array       | length */
        JsClassMappedArguments, /*               | length */
        JsClassDate, /* u.object_data */
        JsClassModuleNs,
        JsClassCFunction, /* u.cfunc */
        JsClassBytecodeFunction, /* u.func */
        JsClassBoundFunction, /* u.bound_function */
        JsClassCFunctionData, /* u.c_function_data_record */
        JsClassGeneratorFunction, /* u.func */
        JsClassForInIterator, /* u.for_in_iterator */
        JsClassRegexp, /* u.regexp */
        JsClassArrayBuffer, /* u.array_buffer */
        JsClassSharedArrayBuffer, /* u.array_buffer */
        JsClassUint8CArray, /* u.array (typed_array) */
        JsClassInt8Array, /* u.array (typed_array) */
        JsClassUint8Array, /* u.array (typed_array) */
        JsClassInt16Array, /* u.array (typed_array) */
        JsClassUint16Array, /* u.array (typed_array) */
        JsClassInt32Array, /* u.array (typed_array) */
        JsClassUint32Array, /* u.array (typed_array) */

        //#ifdef CONFIG_BIGNUM
        //    JS_CLASS_BIG_INT64_ARRAY,   /* u.array (typed_array) */
        //    JS_CLASS_BIG_UINT64_ARRAY,  /* u.array (typed_array) */
        //#endif
        JsClassFloat32Array, /* u.array (typed_array) */
        JsClassFloat64Array, /* u.array (typed_array) */
        JsClassDataview, /* u.typed_array */

        //#ifdef CONFIG_BIGNUM
        //    JS_CLASS_BIG_INT,           /* u.object_data */
        //    JS_CLASS_BIG_FLOAT,         /* u.object_data */
        //    JS_CLASS_FLOAT_ENV,         /* u.float_env */
        //    JS_CLASS_BIG_DECIMAL,       /* u.object_data */
        //    JS_CLASS_OPERATOR_SET,      /* u.operator_set */
        //#endif
        JsClassMap, /* u.map_state */
        JsClassSet, /* u.map_state */
        JsClassWeakmap, /* u.map_state */
        JsClassWeakset, /* u.map_state */
        JsClassMapIterator, /* u.map_iterator_data */
        JsClassSetIterator, /* u.map_iterator_data */
        JsClassArrayIterator, /* u.array_iterator_data */
        JsClassStringIterator, /* u.array_iterator_data */
        JsClassRegexpStringIterator, /* u.regexp_string_iterator_data */
        JsClassGenerator, /* u.generator_data */
        JsClassProxy, /* u.proxy_data */
        JsClassPromise, /* u.promise_data */
        JsClassPromiseResolveFunction, /* u.promise_function_data */
        JsClassPromiseRejectFunction, /* u.promise_function_data */
        JsClassAsyncFunction, /* u.func */
        JsClassAsyncFunctionResolve, /* u.async_function_data */
        JsClassAsyncFunctionReject, /* u.async_function_data */
        JsClassAsyncFromSyncIterator, /* u.async_from_sync_iterator_data */
        JsClassAsyncGeneratorFunction, /* u.func */
        JsClassAsyncGenerator, /* u.async_generator_data */

        JsClassInitCount, /* last entry for predefined classes */
    }
}
