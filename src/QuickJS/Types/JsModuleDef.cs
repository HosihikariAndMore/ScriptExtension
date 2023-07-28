using System.Text.Json.Nodes;

namespace Hosihikari.VanillaScript.QuickJS.Types;

//ref #L773
public ref struct JsModuleDef
{
    /*
     struct JSModuleDef {
    JSRefCountHeader header; // must come first, 32-bit
    JSAtom module_name;
    struct list_head link;

    JSReqModuleEntry* req_module_entries;
    int req_module_entries_count;
    int req_module_entries_size;

    JSExportEntry* export_entries;
    int export_entries_count;
    int export_entries_size;

    JSStarExportEntry* star_export_entries;
    int star_export_entries_count;
    int star_export_entries_size;

    JSImportEntry* import_entries;
    int import_entries_count;
    int import_entries_size;

    JSValue module_ns;
    JSValue func_obj; // only used for JS modules
    JSModuleInitFunc* init_func; // only used for C modules
    BOOL resolved : 8;
    BOOL func_created : 8;
    BOOL instantiated : 8;
    BOOL evaluated : 8;
    BOOL eval_mark : 8; // temporary use during js_evaluate_module()
    // true if evaluation yielded an exception. It is saved in
    //   eval_exception
    BOOL eval_has_exception : 8;
    JSValue eval_exception;
    JSValue meta_obj; // for import.meta
};
     */
}
