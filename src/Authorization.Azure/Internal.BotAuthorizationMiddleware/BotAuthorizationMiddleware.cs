using System;
using GGroupp.Platform;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

using IAzureUserGetFunc = IAsyncValueFunc<AzureUserMeGetIn, Result<AzureUserGetOut, Failure<AzureUserGetFailureCode>>>;
using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

internal sealed partial class BotAuthorizationMiddleware : IAsyncValueFunc<IBotContext, Unit>
{
    private const string FlowId = "UserSignIn";

    private readonly IAzureUserGetFunc azureUserGetFunc;

    private readonly IBotUserGetFunc botUserGetFunc;

    private readonly BotAuthorizationOption option;

    internal BotAuthorizationMiddleware(IAzureUserGetFunc azureUserGetFunc, IBotUserGetFunc botUserGetFunc, BotAuthorizationOption option)
    {
        this.azureUserGetFunc = azureUserGetFunc;
        this.botUserGetFunc = botUserGetFunc;
        this.option = option;
    }

    private static IOAuthFlowContext CreateFlowContext(IBotContext botContext)
        =>
        new OAuthFlowContextImpl(
            turnContext: botContext.TurnContext,
            logger: botContext.LoggerFactory.CreateLogger<BotAuthorizationMiddleware>());
}