using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

partial class OAuthFlowExtensions
{
    internal static async ValueTask<Result<BotUser, BotFlowFailure>> AuthorizeInTeamsAsync(
        this IOAuthFlowContext context, IBotUserGetFunc botUserGetFunc, BotAuthorizationOption option, CancellationToken cancellationToken)
    {
        var logger = context.GetLogger();

        try
        {
            var memberId = context.Activity.From.Id;
            var member = await TeamsInfo.GetMemberAsync(context, memberId, cancellationToken).ConfigureAwait(false);

            if (member is null)
            {
                return new BotFlowFailure(
                    userMessage: option.UnexpectedFailureMessage,
                    logMessage: $"Teams member cannot be found by Id: {memberId}");
            }

            var azureUser = new AzureUserGetOut(
                id: Guid.Parse(member.AadObjectId),
                mail: member.Email,
                displayName: member.Name);

            return await botUserGetFunc.InvokeAsync(azureUser, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Authorization in Teams has finished with an unexpected exception");
            return new BotFlowFailure(
                userMessage: option.UnexpectedFailureMessage,
                logMessage: $"Authorization in Teams has finished with an unexpected exception {ex.GetType().FullName}: {ex.Message}");
        }
    }
}