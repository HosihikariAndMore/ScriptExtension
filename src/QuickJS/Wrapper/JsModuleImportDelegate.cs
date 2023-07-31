using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public delegate JsValue JsModuleImportDelegate(JsContextWrapper ctx);
public delegate AutoDropJsValue JsModuleImportAutoDelegate(JsContextWrapper ctx);
