using Hosihikari.Logging;

namespace Hosihikari.VanillaScript;

internal static class Log
{
    private static readonly Lazy<Logger> _logger =
        new(() =>
        {
            var instance = new Logger(nameof(VanillaScript));
            instance.SetupConsole();
            return instance;
        });

    internal static Logger Logger => _logger.Value;
}
