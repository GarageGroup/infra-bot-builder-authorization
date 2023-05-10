using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

public static class LogoutBotBuilder
{
    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName).InvokeAsync(context, token);
    }

    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName, Func<IBotContext, BotLogoutOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName, optionResolver.Invoke(context)).InvokeAsync(context, token);
    }

    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName, Func<BotLogoutOption> optionFactory)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionFactory);

        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName, optionFactory.Invoke()).InvokeAsync(context, token);
    }
}