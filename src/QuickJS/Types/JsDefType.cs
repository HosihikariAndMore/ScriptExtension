namespace Hosihikari.ScriptExtension.QuickJS.Types;

//ref #L995
public enum JsDefType : byte
{
    //#define JS_DEF_CFUNC          0
    CFunc = 0,

    //#define JS_DEF_CGETSET        1
    CGetSet = 1,

    //#define JS_DEF_CGETSET_MAGIC  2
    CGetSetMagic = 2,

    //#define JS_DEF_PROP_STRING    3
    PropString = 3,

    //#define JS_DEF_PROP_INT32     4
    PropInt32 = 4,

    //#define JS_DEF_PROP_INT64     5
    PropInt64 = 5,

    //#define JS_DEF_PROP_DOUBLE    6
    PropDouble = 6,

    //#define JS_DEF_PROP_UNDEFINED 7
    PropUndefined = 7,

    //#define JS_DEF_OBJECT         8
    Object = 8,

    //#define JS_DEF_ALIAS          9
    Alias = 9,
}
