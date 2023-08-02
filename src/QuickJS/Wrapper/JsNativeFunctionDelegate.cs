using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public delegate JsValue JsNativeFunctionDelegate(
    JsContextWrapper ctx,
    JsValue thisObject,
    ReadOnlySpan<JsValue> argv
);
