using Flandre.Core.Messaging;

#pragma warning disable CS1591

namespace Flandre.Framework.Common;

public static class CommandExtensions
{
    #region WithAction (sync)

    public static Command WithAction(this Command command,
        Func<CommandContext, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1>(this Command command,
        Func<CommandContext, TParam1, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2>(this Command command,
        Func<CommandContext, TParam1, TParam2, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, MessageContent> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, MessageContent>
            action)
        => command.WithAction(action.Method);

    #endregion

    #region WithAction (Task)

    public static Command WithAction(this Command command,
        Func<CommandContext, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1>(this Command command,
        Func<CommandContext, TParam1, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2>(this Command command,
        Func<CommandContext, TParam1, TParam2, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task>
            action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
            Task> action)
        => command.WithAction(action.Method);

    #endregion

    #region WithAction (sync, no return type)

    public static Command WithAction(this Command command,
        Action<CommandContext> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1>(this Command command,
        Action<CommandContext, TParam1> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2>(this Command command,
        Action<CommandContext, TParam1, TParam2> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3>(this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4>(this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3, TParam4> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5>(this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this Command command,
        Action<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
            action)
        => command.WithAction(action.Method);

    #endregion

    #region WithAction (Task)

    public static Command WithAction(this Command command,
        Func<CommandContext, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1>(this Command command,
        Func<CommandContext, TParam1, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2>(this Command command,
        Func<CommandContext, TParam1, TParam2, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<MessageContent>> action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<MessageContent>>
            action)
        => command.WithAction(action.Method);

    public static Command WithAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this Command command,
        Func<CommandContext, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
            Task<MessageContent>> action)
        => command.WithAction(action.Method);

    #endregion
}
