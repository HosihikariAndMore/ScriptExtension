namespace Hosihikari.ScriptExtension.QuickJS.Types;

//ref #L867
public ref struct JsObject
{
    //       union {
    //        JSGCObjectHeader header;
    //        struct {
    //            int __gc_ref_count; /* corresponds to header.ref_count */
    //            uint8_t __gc_mark; /* corresponds to header.mark/gc_obj_type */

    //            uint8_t extensible : 1;
    //            uint8_t free_mark : 1; /* only used when freeing objects with cycles */
    //            uint8_t is_exotic : 1; /* TRUE if object has exotic property handlers */
    //            uint8_t fast_array : 1; /* TRUE if u.array is used for get/put (for JS_CLASS_ARRAY, JS_CLASS_ARGUMENTS and typed arrays) */
    //            uint8_t is_constructor : 1; /* TRUE if object is a constructor function */
    //            uint8_t is_uncatchable_error : 1; /* if TRUE, error is not catchable */
    //            uint8_t tmp_mark : 1; /* used in JS_WriteObjectRec() */
    //            uint8_t is_HTMLDDA : 1; /* specific annex B IsHtmlDDA behavior */
    //            uint16_t class_id; /* see JS_CLASS_x */
    //        };
    //    };
    //    /* byte offsets: 16/24 */
    //    JSShape *shape; /* prototype and property names + flag */
    //    JSProperty *prop; /* array of properties */
    //    /* byte offsets: 24/40 */
    //    struct JSMapRecord *first_weak_ref; /* XXX: use a bit and an external hash table? */
    //    /* byte offsets: 28/48 */
    //    union {
    //        void *opaque;
    //        struct JSBoundFunction *bound_function; /* JS_CLASS_BOUND_FUNCTION */
    //        struct JSCFunctionDataRecord *c_function_data_record; /* JS_CLASS_C_FUNCTION_DATA */
    //        struct JSForInIterator *for_in_iterator; /* JS_CLASS_FOR_IN_ITERATOR */
    //        struct JSArrayBuffer *array_buffer; /* JS_CLASS_ARRAY_BUFFER, JS_CLASS_SHARED_ARRAY_BUFFER */
    //        struct JSTypedArray *typed_array; /* JS_CLASS_UINT8C_ARRAY..JS_CLASS_DATAVIEW */
    //#ifdef CONFIG_BIGNUM
    //        struct JSFloatEnv *float_env; /* JS_CLASS_FLOAT_ENV */
    //        struct JSOperatorSetData *operator_set; /* JS_CLASS_OPERATOR_SET */
    //#endif
    //        struct JSMapState *map_state;   /* JS_CLASS_MAP..JS_CLASS_WEAKSET */
    //        struct JSMapIteratorData *map_iterator_data; /* JS_CLASS_MAP_ITERATOR, JS_CLASS_SET_ITERATOR */
    //        struct JSArrayIteratorData *array_iterator_data; /* JS_CLASS_ARRAY_ITERATOR, JS_CLASS_STRING_ITERATOR */
    //        struct JSRegExpStringIteratorData *regexp_string_iterator_data; /* JS_CLASS_REGEXP_STRING_ITERATOR */
    //        struct JSGeneratorData *generator_data; /* JS_CLASS_GENERATOR */
    //        struct JSProxyData *proxy_data; /* JS_CLASS_PROXY */
    //        struct JSPromiseData *promise_data; /* JS_CLASS_PROMISE */
    //        struct JSPromiseFunctionData *promise_function_data; /* JS_CLASS_PROMISE_RESOLVE_FUNCTION, JS_CLASS_PROMISE_REJECT_FUNCTION */
    //        struct JSAsyncFunctionData *async_function_data; /* JS_CLASS_ASYNC_FUNCTION_RESOLVE, JS_CLASS_ASYNC_FUNCTION_REJECT */
    //        struct JSAsyncFromSyncIteratorData *async_from_sync_iterator_data; /* JS_CLASS_ASYNC_FROM_SYNC_ITERATOR */
    //        struct JSAsyncGeneratorData *async_generator_data; /* JS_CLASS_ASYNC_GENERATOR */
    //        struct { /* JS_CLASS_BYTECODE_FUNCTION: 12/24 bytes */
    //            /* also used by JS_CLASS_GENERATOR_FUNCTION, JS_CLASS_ASYNC_FUNCTION and JS_CLASS_ASYNC_GENERATOR_FUNCTION */
    //            struct JSFunctionBytecode *function_bytecode;
    //            JSVarRef **var_refs;
    //            JSObject *home_object; /* for 'super' access */
    //        } func;
    //        struct { /* JS_CLASS_C_FUNCTION: 12/20 bytes */
    //            JSContext *realm;
    //            JSCFunctionType c_function;
    //            uint8_t length;
    //            uint8_t cproto;
    //            int16_t magic;
    //        } cfunc;
    //        /* array part for fast arrays and typed arrays */
    //        struct { /* JS_CLASS_ARRAY, JS_CLASS_ARGUMENTS, JS_CLASS_UINT8C_ARRAY..JS_CLASS_FLOAT64_ARRAY */
    //            union {
    //                uint32_t size;          /* JS_CLASS_ARRAY, JS_CLASS_ARGUMENTS */
    //                struct JSTypedArray *typed_array; /* JS_CLASS_UINT8C_ARRAY..JS_CLASS_FLOAT64_ARRAY */
    //            } u1;
    //            union {
    //                JSValue *values;        /* JS_CLASS_ARRAY, JS_CLASS_ARGUMENTS */
    //                void *ptr;              /* JS_CLASS_UINT8C_ARRAY..JS_CLASS_FLOAT64_ARRAY */
    //                int8_t *int8_ptr;       /* JS_CLASS_INT8_ARRAY */
    //                uint8_t *uint8_ptr;     /* JS_CLASS_UINT8_ARRAY, JS_CLASS_UINT8C_ARRAY */
    //                int16_t *int16_ptr;     /* JS_CLASS_INT16_ARRAY */
    //                uint16_t *uint16_ptr;   /* JS_CLASS_UINT16_ARRAY */
    //                int32_t *int32_ptr;     /* JS_CLASS_INT32_ARRAY */
    //                uint32_t *uint32_ptr;   /* JS_CLASS_UINT32_ARRAY */
    //                int64_t *int64_ptr;     /* JS_CLASS_INT64_ARRAY */
    //                uint64_t *uint64_ptr;   /* JS_CLASS_UINT64_ARRAY */
    //                float *float_ptr;       /* JS_CLASS_FLOAT32_ARRAY */
    //                double *double_ptr;     /* JS_CLASS_FLOAT64_ARRAY */
    //            } u;
    //            uint32_t count; /* <= 2^31-1. 0 for a detached typed array */
    //        } array;    /* 12/20 bytes */
    //        JSRegExp regexp;    /* JS_CLASS_REGEXP: 8/16 bytes */
    //        JSValue object_data;    /* for JS_SetObjectData(): 8/16/16 bytes */
    //    } u;
    //    /* byte sizes: 40/48/72 */
}
