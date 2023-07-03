using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

public static class AuthorizationBotBuilder
{
    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetSupplier> azureUserApiResolver,
        Func<IBotContext, IBotUserGetFunc> botUserGetFuncResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);
        ArgumentNullException.ThrowIfNull(azureUserApiResolver);
        ArgumentNullException.ThrowIfNull(botUserGetFuncResolver);

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserApiResolver, botUserGetFuncResolver);
    }

    public static IBotBuilder UseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetSupplier> azureUserApiResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);
        ArgumentNullException.ThrowIfNull(azureUserApiResolver);

        return InnerUseAuthorization(botBuilder, optionResolver, azureUserApiResolver);
    }

    private static IBotBuilder InnerUseAuthorization(
        IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetSupplier> azureUserApiResolver,
        Func<IBotContext, IBotUserGetFunc>? botUserGetFuncResolver = null)
    {
        return botBuilder.Use(InnerInvokeAsync);

        ValueTask<Unit> InnerInvokeAsync(IBotContext context, CancellationToken token)
            =>
            InnerResolveMiddleware(context).InvokeAsync(context, token);

        IAsyncValueFunc<IBotContext, Unit> InnerResolveMiddleware(IBotContext context)
            =>
            new BotAuthorizationMiddleware(
                azureUserApi: azureUserApiResolver.Invoke(context),
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