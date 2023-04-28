using Flandre.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Flandre.Framework.Routing;

/// <summary>
/// 指令路由构造器的扩展方法
/// </summary>
public static class CommandRouteBuilderExtensions
{
    /// <summary>
    /// 添加指令
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="fullname"></param>
    /// <param name="commandDelegate"></param>
    public static void MapCommand(this ICommandRouteBuilder builder, string fullname, Delegate commandDelegate)
    {
        var cmdService = builder.Services.GetRequiredService<CommandService>();
        cmdService.RootCommandNode.MapCommand(null, fullname)
            .WithAction(commandDelegate);
    }
}
