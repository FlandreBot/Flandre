namespace Flandre.Framework.Routing;

/// <summary>
/// 指令路由构造器
/// </summary>
public interface ICommandRouteBuilder
{
    /// <summary>
    /// 服务提供源
    /// </summary>
    IServiceProvider Services { get; }
}
