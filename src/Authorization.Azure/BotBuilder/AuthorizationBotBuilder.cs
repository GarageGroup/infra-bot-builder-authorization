using System;
using System.Threading;
using System.Threading.Tasks;
using GGroupp.Platform;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GGroupp.Infra.Bot.Builder;

using IAzureUserGetFunc = IAsyncValueFunc<AzureUserMeGetIn, Result<AzureUserGetOut, Failure<AzureUserGetFailureCode>>>;
using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

public static class AuthorizationBotBuilder
{
    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetFunc> azureUserGetFuncResolver,
        Func<IBotContext, IBotUserGetFunc> botUserGetFuncResolver)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));
        _ = azureUserGetFuncResolver ?? throw new ArgumentNullException(nameof(azureUserGetFuncResolver));
        _ = botUserGetFuncResolver ?? throw new ArgumentNullException(nameof(botUserGetFuncResolver));

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserGetFuncResolver, botUserGetFuncResolver);
    }

    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetFunc> azureUserGetFuncResolver)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));
        _ = azureUserGetFuncResolver ?? throw new ArgumentNullException(nameof(azureUserGetFuncResolver));

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserGetFuncResolver);
    }

    private static IBotBuilder InnerUseAuthorization(
        IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetFunc> azureUserGetFuncResolver,
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
        _ = context ?? throw new ArgumentNullException(nameof(context));

        return new(
            context.ServiceProvider.GetService<IConfiguration>()?.GetValue<string>("OAuthConnectionName") ?? string.Empty);
    }
}