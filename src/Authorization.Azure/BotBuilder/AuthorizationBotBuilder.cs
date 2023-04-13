using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GGroupp.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

public static class AuthorizationBotBuilder
{
    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserMeGetFunc> azureUserGetFuncResolver,
        Func<IBotContext, IBotUserGetFunc> botUserGetFuncResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);
        ArgumentNullException.ThrowIfNull(azureUserGetFuncResolver);
        ArgumentNullException.ThrowIfNull(botUserGetFuncResolver);

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserGetFuncResolver, botUserGetFuncResolver);
    }

    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserMeGetFunc> azureUserGetFuncResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);
        ArgumentNullException.ThrowIfNull(azureUserGetFuncResolver);

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserGetFuncResolver);
    }

    private static IBotBuilder InnerUseAuthorization(
        IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserMeGetFunc> azureUserGetFuncResolver,
        Func<IBotContext, IBotUserGetFunc>? botUserGetFuncResolver = null)
    {
        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            InnerResolveMiddleware(context).InvokeAsync(context, token);

        IAsyncValueFunc<IBotContext, Unit> InnerResolveMiddleware(IBotContext context)
            =>
            new BotAuthorizationMiddleware(
                azureUserGetFunc: azureUserGetFuncResolver.Invoke(context),
                botUserGetFunc: botUserGetFuncResolver?.Invoke(context) ?? BotUserGetFunc.Instance,
                option: optionResolver.Invoke(context));
    }

    public static BotAuthorizationOption ResolveStandardOption(this IBotContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new(
            context.ServiceProvider.GetService<IConfiguration>()?.GetValue<string>("OAuthConnectionName") ?? string.Empty);
    }
}