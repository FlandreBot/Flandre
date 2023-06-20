using Flandre.Framework.Common;

namespace Flandre.Framework.Services;

/// <summary>
/// 中间件服务
/// </summary>
public sealed class MiddlewareService
{
    private readonly List<Func<MiddlewareContext, Func<Task>, Task>> _middleware = new();
}
