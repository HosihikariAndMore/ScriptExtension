namespace Hosihikari.ScriptExtension.QuickJS.Types;

//typedef struct JSClassDef
public unsafe ref struct JsClassDef
{
    //    const char* class_name;
    public byte* ClassName;

    //    JSClassFinalizer* finalizer;
    //typedef void JSClassFinalizer(JSRuntime *rt, JSValue val);
    public delegate* unmanaged<JsRuntime*, JsValue, void> Finalizer;

    //    JSClassGCMark* gc_mark;
    //typedef void JSClassGCMark(JSRuntime *rt, JSValueConst val,JS_MarkFunc* mark_func);
    //typedef void JS_MarkFunc(JSRuntime* rt, JSGCObjectHeader* gp);
    /// <summary>
    /// mark all JsValue owned by current object
    /// call Js_MarkValue in this function
    /// </summary>
    public delegate* unmanaged<
        JsRuntime*,
        JsValue,
        delegate* unmanaged<JsRuntime*, JsGCObjectHeader*, void>,
        void> GcMark;

    //    /*  */
    //    JSClassCall* call;
    //typedef JSValue JSClassCall(JSContext *ctx, JSValueConst func_obj,JSValueConst this_val, int argc, JSValueConst *argv,int flags);
    /// <summary>
    /// if call != NULL, the object is a function. If (flags &
    ///       JS_CALL_FLAG_CONSTRUCTOR) != 0, the function is called as a
    ///       constructor. In this case, 'this_val' is new.target. A
    ///       constructor call only happens if the object constructor bit is
    ///       set (see JS_SetConstructorBit()).
    /// </summary>
    public delegate* unmanaged<
        JsContext*,
        JsValue,
        JsValue,
        int,
        JsValue*,
        JsCallFlag,
        JsValue> Call;

    public JsClassExoticMethods* Exotic;

    #region JsClassExoticMethods
    //typedef struct JSClassExoticMethods
    public struct JsClassExoticMethods
    {
        //    int (* get_own_property) (JSContext* ctx, JSPropertyDescriptor* desc,
        //        JSValueConst obj, JSAtom prop);
        /// <summary>
        ///Return -1 if exception (can only happen in case of Proxy object),
        ///       FALSE if the property does not exists, TRUE if it exists. If 1 is
        ///       returned, the property descriptor 'desc' is filled if != NULL.
        /// </summary>
        public delegate* unmanaged<
            JsContext*,
            JsPropertyDescriptor*,
            JsValue,
            JsAtom,
            int> GetOwnProperty;

        //    int (* get_own_property_names) (JSContext* ctx, JSPropertyEnum** ptab,
        //        uint32_t* plen,
        //        JSValueConst obj);
        /// <summary>
        /// '*ptab' should hold the '*plen' property keys. Return 0 if OK,
        ///       -1 if exception. The 'is_enumerable' field is ignored.
        /// </summary>
        public delegate* unmanaged<
            JsContext*,
            JsPropertyEnum**,
            uint*,
            JsValue,
            int> GetOwnPropertyNames;

        //    int (* delete_property) (JSContext* ctx, JSValueConst obj, JSAtom prop);
        /// <summary>
        /// return &lt; 0 if exception, or TRUE/FALSE
        /// </summary>
        public delegate* unmanaged<JsContext*, JsValue, JsAtom, int> DeleteProperty;

        //    int (* define_own_property) (JSContext* ctx, JSValueConst this_obj,
        //        JSAtom prop, JSValueConst val,
        //        JSValueConst getter, JSValueConst setter,
        //        int flags);
        /// <summary>
        /// return &lt; 0 if exception or TRUE/FALSE
        /// </summary>
        public delegate* unmanaged<
            JsContext*,
            JsValue, //this_obj
            JsAtom, //prop
            JsValue, //val
            JsValue, //getter
            JsValue, //setter
            JsPropertyFlags, //flags
            int> DefineOwnProperty;

        /* The following methods can be emulated with the previous ones,
so they are usually not needed */

        //    int (* has_property) (JSContext* ctx, JSValueConst obj, JSAtom atom);
        /// <summary>
        /// The following methods can be emulated with the previous ones,so they are usually not needed
        /// return &lt; 0 if exception or TRUE/FALSE
        /// </summary>
        public delegate* unmanaged<JsContext*, JsValue, JsAtom, int> HasProperty;

        //    JSValue(*get_property)(JSContext* ctx, JSValueConst obj, JSAtom atom,
        //        JSValueConst receiver);
        /// <summary>
        /// The following methods can be emulated with the previous ones,so they are usually not needed
        /// </summary>
        public delegate* unmanaged<JsContext*, JsValue, JsAtom, JsValue, JsValue> GetProperty;

        //    int (* set_property) (JSContext* ctx, JSValueConst obj, JSAtom atom,
        //        JSValueConst value, JSValueConst receiver, int flags);
        /// <summary>
        /// return &lt; 0 if exception or TRUE/FALSE
        /// The following methods can be emulated with the previous ones,so they are usually not needed
        /// </summary>
        public delegate* unmanaged<
            JsContext*,
            JsValue, //obj
            JsAtom, //atom
            JsValue, //value
            JsValue, //receiver
            JsPropertyFlags, //flags
            int> SetProperty;
    }
    //JSClassExoticMethods;
    #endregion
}
//JSClassDef;
