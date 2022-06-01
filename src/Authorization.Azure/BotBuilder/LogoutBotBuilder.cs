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

    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName, Func<IBotContext, BotLogoutOption> optionResolver)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));

        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName, optionResolver.Invoke(context)).InvokeAsync(context, token);
    }

    public static IBotBuilder UseLogout(this IBotBuilder botBuilder, string commandName, Func<BotLogoutOption> optionFactory)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));

        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            new BotLogoutMiddleware(commandName, optionFactory.Invoke()).InvokeAsync(context, token);
    }
}