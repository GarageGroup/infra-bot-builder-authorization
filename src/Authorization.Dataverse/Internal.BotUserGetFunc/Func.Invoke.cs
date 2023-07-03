using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class BotDataverseUserGetFunc
{
    public ValueTask<Result<BotUser, BotFlowFailure>> InvokeAsync(AzureUserGetOut azureUser, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<BotUser, BotFlowFailure>>(cancellationToken);
        }

        return InnerInvokeAsync(azureUser, cancellationToken);
    }

    private async ValueTask<Result<BotUser, BotFlowFailure>> InnerInvokeAsync(AzureUserGetOut azureUser, CancellationToken cancellationToken)
    {
        var dataverseResult = await dataverseUserApi.GetUserAsync(new(azureUser.Id), cancellationToken).ConfigureAwait(false);
        return dataverseResult.Map(MapDataverseUser, MapFailure);

        BotUser MapDataverseUser(DataverseUserGetOut dataverseUser)
            =>
            dataverseUser.ToUserDataJson(azureUser);

        BotFlowFailure MapFailure(Failure<DataverseUserGetFailureCode> failure)
            =>
            new(
                userMessage: GetUserMessage(failure.FailureCode),
                logMessage: $"Dataverse authoriation has failed with a message: {failure.FailureMessage}");
    }

    private static string GetUserMessage(DataverseUserGetFailureCode failureCode)
        =>
        failureCode switch
        {
            DataverseUserGetFailureCode.NotFound => UserNotFoundFailureMessage,
            _ => UnexpectedFailureMessage
        };
}