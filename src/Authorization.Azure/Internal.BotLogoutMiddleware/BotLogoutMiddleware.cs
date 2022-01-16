using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

internal sealed partial class BotLogoutMiddleware : IAsyncValueFunc<IBotContext, Unit>
{
    private const string DefaultCommandName = "logout";

    private readonly string commandName;

    internal BotLogoutMiddleware([AllowNull] string commandName = DefaultCommandName)
        =>
        this.commandName = string.IsNullOrEmpty(commandName) ? DefaultCommandName : commandName;
}