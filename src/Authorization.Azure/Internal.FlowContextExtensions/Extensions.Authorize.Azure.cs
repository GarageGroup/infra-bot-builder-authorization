using System;
using System.Threading;
using System.Threading.Tasks;
using GGroupp.Platform;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

using IAzureUserGetFunc = IAsyncValueFunc<AzureUserMeGetIn, Result<AzureUserGetOut, Failure<AzureUserGetFailureCode>>>;
using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

partial class OAuthFlowContextExtensions
{
    internal static async ValueTask<Result<BotUser, BotFlowFailure>> AuthorizeInAzureAsync(
        this IOAuthFlowContext context,
        IAzureUserGetFunc azureUserGetFunc,
        IBotUserGetFunc botUserGetFunc,
        TokenResponse tokenResponse,
        BotAuthorizationOption option,
        CancellationToken cancellationToken)
    {
        var azureIn = new AzureUserMeGetIn(tokenResponse.Token);
        var azureResult = await azureUserGetFunc.InvokeAsync(azureIn, cancellationToken).ConfigureAwait(false);

        return await azureResult.MapFailure(MapFailure).ForwardValueAsync(ToBotUserAsync).ConfigureAwait(false);

        ValueTask<Result<BotUser, BotFlowFailure>> ToBotUserAsync(AzureUserGetOut azureUser)
            =>
            botUserGetFunc.InvokeAsync(azureUser, cancellationToken);

        BotFlowFailure MapFailure(Failure<AzureUserGetFailureCode> failure)
            =>
            new(
                userMessage: option.UnexpectedFailureMessage,
                logMessage: $"Azure authoriation has failed with message: {failure.FailureMessage}");
    }
}