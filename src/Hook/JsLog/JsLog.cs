using System.Runtime.InteropServices;
using System.Text;
using Hosihikari.ScriptExtension.Assets;
using Hosihikari.ScriptExtension.QuickJS;
using Hosihikari.ScriptExtension.QuickJS.Extensions;
using Hosihikari.ScriptExtension.QuickJS.Helper;
using Hosihikari.ScriptExtension.QuickJS.Types;
using Hosihikari.ScriptExtension.Scripting;

namespace Hosihikari.ScriptExtension.Hook.JsLog;

internal class JsLog
{
    private static Dictionary<string, Logger> _loggers = new();

    private static unsafe Logger GetJsLogger(JsContext* ctx)
    {
        var file = Native.JS_GetScriptOrModuleName(ctx, 1);
        if (string.IsNullOrWhiteSpace(file) || file == Prepare.EntryPointJsName)
        {
            return Log.Logger;
        }
        if (_loggers.TryGetValue(file, out var logger))
            return logger;
        logger = new Logger(file);
        logger.SetupConsole();
        _loggers.Add(file, logger);
        return logger;
    }

    public static unsafe void Bind(JsContext* ctx, JsValue globalObject)
    {
        using var consoleInstance = Native.JS_NewObject(ctx);
        var console = consoleInstance.Steal();
        #region Trace
        console.DefineFunction(ctx, "trace", &PrintTrace, 1, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue PrintTrace(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            try
            {
                var (file, line) = GetJsSourceInfo(ctx);
                GetJsLogger(ctx)
                    .Trace(
                        ParseLog(ctx, new ReadOnlySpan<JsValue>(argvIn, argCount)),
                        sourceFile: file,
                        sourceLine: line
                    );
            }
            catch (Exception ex)
            {
                GetJsLogger(ctx).Error("PrintTrace Invoke Failed", ex);
            }
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        #region Info
        globalObject.DefineFunction(ctx, "print", &PrintInfo, 1, flags: JsPropertyFlags.Normal);
        console.DefineFunction(ctx, "log", &PrintInfo, 1, flags: JsPropertyFlags.Normal);
        console.DefineFunction(ctx, "info", &PrintInfo, 1, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue PrintInfo(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            try
            {
                GetJsLogger(ctx)
                    .Information(ParseLog(ctx, new ReadOnlySpan<JsValue>(argvIn, argCount)));
            }
            catch (Exception ex)
            {
                GetJsLogger(ctx).Error("PrintInfo Invoke Failed", ex);
            }
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        #region Debug
        console.DefineFunction(ctx, "debug", &PrintDebug, 1, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue PrintDebug(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            try
            {
                GetJsLogger(ctx).Debug(ParseLog(ctx, new ReadOnlySpan<JsValue>(argvIn, argCount)));
            }
            catch (Exception ex)
            {
                GetJsLogger(ctx).Error("PrintDebug Invoke Failed", ex);
            }
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        #region Warn
        console.DefineFunction(ctx, "warn", &PrintWarn, 1, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue PrintWarn(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            try
            {
                GetJsLogger(ctx)
                    .Warning(ParseLog(ctx, new ReadOnlySpan<JsValue>(argvIn, argCount)));
            }
            catch (Exception ex)
            {
                GetJsLogger(ctx).Error("PrintWarn Invoke Failed", ex);
            }
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        #region Error
        console.DefineFunction(ctx, "error", &PrintError, 1, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue PrintError(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            try
            {
                var (file, line) = GetJsSourceInfo(ctx);
                GetJsLogger(ctx)
                    .Error(
                        ParseLog(ctx, new ReadOnlySpan<JsValue>(argvIn, argCount)),
                        sourceFile: file,
                        sourceLine: line
                    );
            }
            catch (Exception ex)
            {
                GetJsLogger(ctx).Error("PrintError Invoke Failed", ex);
            }
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        #region Assert
        //todo Assert
        //console.DefineFunction(ctx, "assert", &Assert, 2, flags: JsPropertyFlags.Normal);

        //[UnmanagedCallersOnly]
        //static JsValue Assert(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        //{
        //    var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
        //    if (argv[0].IsFalsey(ctx))
        //    {
        //        GetJsLogger(ctx).Error(ParseLog(ctx, argv.Slice(1)));
        //    }
        //    return JsValueCreateHelper.Undefined;
        //}
        #endregion
        #region Clear
        console.DefineFunction(ctx, "clear", &Clear, 0, flags: JsPropertyFlags.Normal);
        [UnmanagedCallersOnly]
        static JsValue Clear(JsContext* ctx, JsValue thisObj, int argCount, JsValue* argvIn)
        {
            Console.Clear();
            return JsValueCreateHelper.Undefined;
        }
        #endregion
        globalObject.DefineProperty(ctx, "console", console, flags: JsPropertyFlags.Normal);
    }

    static unsafe (string file, int line) GetJsSourceInfo(JsContext* ctx)
    {
        try //get full stack from Error
        {
            Native.JS_ThrowInternalError(ctx, "");
            using var error = Native.JS_GetException(ctx);
            var stack = error.Value.GetStringProperty(ctx, JsAtom.BuildIn.Stack);
            //     at trace (native)
            //    at <anonymous> (test.js:3)
            var lineStr = stack.Split('\n')[1]; //at <anonymous> (test.js:3)
            var source = lineStr[(lineStr.LastIndexOf('(') + 1)..^1]; //test.js:3
            var data = source.Split(":");
            if (data.Length > 1)
                return (data[0], int.Parse(data[1]));
            return (data[0], -1);
        }
        catch //get only file name from JS_GetScriptOrModuleName
        {
            var file = Native.JS_GetScriptOrModuleName(ctx, 1);
            return (file, -1);
        }
    }

    static unsafe string ParseLog(JsContext* ctx, ReadOnlySpan<JsValue> argv)
    {
        if (argv.Length == 0)
        {
            return "[empty]";
        }
        var sb = new StringBuilder();
        foreach (var arg in argv)
        {
            if (
                arg.Tag == JsTag.Object
                && arg.GetClassName(ctx) is var className
                && !string.IsNullOrWhiteSpace(className)
            )
            {
                sb.Append('<');
                sb.Append(className);
                sb.Append('>');
                if (className == "Player")
                {
                    if (McQuickJs.JsValueToPlayer(ctx, arg, out var player))
                    {
                        sb.Append('[');
                        sb.Append(player.Name);
                        sb.Append(']');
                    }
                }
                else
                {
                    sb.Append(arg.ToString(ctx));
                }
            }
            else
            {
                sb.Append(arg.ToString(ctx));
            }
            if (arg.Tag == JsTag.Object && Native.JS_IsError(ctx, arg))
            {
                sb.AppendLine();
                sb.AppendLine(arg.GetStringProperty(ctx, JsAtom.BuildIn.Stack));
            }
            else
            {
                sb.Append(" ");
            }
        }
        return sb.ToString();
    }
}
