using Microsoft.Extensions.Logging;

namespace Flandre.Framework.Utils;

internal static class LogUtils
{
    internal static ILogger<T> GetTempLogger<T>()
    {
        using var factory = new LoggerFactory();
        return factory.CreateLogger<T>();
    }
}