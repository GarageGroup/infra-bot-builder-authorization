using System;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

internal sealed partial class BotAuthorizationMiddleware : IAsyncValueFunc<IBotContext, Unit>
{
    private const string FlowId = "UserSignIn";

    private readonly IAzureUserGetSupplier azureUserApi;

    private readonly IBotUserGetFunc botUserGetFunc;

    private readonly BotAuthorizationOption option;

    internal BotAuthorizationMiddleware(IAzureUserGetSupplier azureUserApi, IBotUserGetFunc botUserGetFunc, BotAuthorizationOption option)
    {
        this.azureUserApi = azureUserApi;
        this.botUserGetFunc = botUserGetFunc;
        this.option = option;
    }

    private static IOAuthFlowContext CreateFlowContext(IBotContext botContext)
        =>
        new OAuthFlowContextImpl(
            turnContext: botContext.TurnContext,
            logger: botContext.LoggerFactory.CreateLogger<BotAuthorizationMiddleware>());
}