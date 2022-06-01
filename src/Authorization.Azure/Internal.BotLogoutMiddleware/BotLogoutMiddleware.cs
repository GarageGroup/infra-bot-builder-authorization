using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

internal sealed partial class BotLogoutMiddleware : IAsyncValueFunc<IBotContext, Unit>
{
    private const string DefaultCommandName = "logout";

    private readonly string commandName;

    private readonly BotLogoutOption logoutOption;

    internal BotLogoutMiddleware([AllowNull] string commandName = DefaultCommandName, [AllowNull] BotLogoutOption logoutOption = null)
    {
        this.commandName = string.IsNullOrEmpty(commandName) ? DefaultCommandName : commandName;
        this.logoutOption = logoutOption ?? new();
    }
}