using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Utils;

internal static class ServiceUtils
{
    internal static void AddRange(this IServiceCollection current, IServiceCollection source)
    {
        foreach (var sd in source)
        {
            current.Add(sd);
        }
    }
}
