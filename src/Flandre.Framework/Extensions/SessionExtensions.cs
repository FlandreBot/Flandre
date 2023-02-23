using Flandre.Core.Messaging;
using Flandre.Framework.Common;
using Flandre.Framework.Utils;

namespace Flandre.Framework.Extensions;

public static class SessionExtensions
{
    /// <summary>
    /// 截获用户发送的下一条消息
    /// </summary>
    /// <param name="ctx">当前指令上下文</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>用户发送的下一条消息，如果超时则返回 null。</returns>
    public static Task<Message?> StartSession(this CommandContext ctx, TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);
        var tcs = new TaskCompletionSource<Message?>();

        var mark = ctx.GetUserMark();

        cts.Token.Register(() =>
        {
            ctx.App.CommandSessions.TryRemove(mark, out _);
            tcs.TrySetResult(null);
        });

        ctx.App.CommandSessions[mark] = tcs;

        return tcs.Task;
    }
}
