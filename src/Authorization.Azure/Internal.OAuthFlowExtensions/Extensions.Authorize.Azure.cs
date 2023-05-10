using System;
using System.Threading;
using System.Threading.Tasks;
using GGroupp.Infra;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

partial class OAuthFlowExtensions
{
    internal static async ValueTask<Result<BotUser, BotFlowFailure>> AuthorizeInAzureAsync(
        this TokenResponse tokenResponse,
        IAzureUserMeGetFunc azureUserGetFunc,
        IBotUserGetFunc botUserGetFunc,
        BotAuthorizationOption option,
        CancellationToken cancellationToken)
    {
        var azureIn = new AzureUserMeGetIn(tokenResponse.Token);
        var azureResult = await azureUserGetFunc.InvokeAsync(azureIn, cancellationToken).ConfigureAwait(false);

        return await azureResult.MapFailure(MapFailure).ForwardValueAsync(ToBotUserAsync).ConfigureAwait(false);

        ValueTask<Result<BotUser, BotFlowFailure>> ToBotUserAsync(AzureUserGetOut azureUser)
            =>
            botUserGetFunc.InvokeAsync(azureUser, cancellationToken);

        BotFlowFailure MapFailure(Failure<Unit> failure)
            =>
            new(
                userMessage: option.UnexpectedFailureMessage,
                logMessage: $"Azure authoriation has failed with message: {failure.FailureMessage}");
    }
}