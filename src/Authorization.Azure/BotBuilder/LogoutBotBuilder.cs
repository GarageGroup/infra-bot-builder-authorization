using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public static class LogoutBotBuilder
{
    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName).InvokeAsync(context, token);
    }
}