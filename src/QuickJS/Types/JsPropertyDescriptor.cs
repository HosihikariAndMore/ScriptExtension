namespace Hosihikari.VanillaScript.QuickJS.Types;

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
    public int Flags;
    public JsValue Value;
    public JsValue Getter;
    public JsValue Setter;
}
