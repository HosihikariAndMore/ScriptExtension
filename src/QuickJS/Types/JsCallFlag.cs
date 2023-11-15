namespace Hosihikari.ScriptExtension.QuickJS.Types;

[Flags]
public enum JsCallFlag
{
    Default = 0,
    Constructor = 1,
    CopyArgv = 2,
    Generator = 1 << 2
}
