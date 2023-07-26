using Hosihikari.Logging;

namespace Hosihikari.VanillaScript;

internal static class Log
{
    internal static Logger Logger { get; } = new(nameof(VanillaScript));
}
