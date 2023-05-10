using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static ValueTask<Result<TokenResponse, BotFlowFailure>> RecognizeTokenOrFailureAsync(
        this IOAuthFlowContext context, BotAuthorizationOption option, CancellationToken cancellationToken)
    {
        if (context.IsNotMessageType())
        {
            return GetUnsuccessfulTokenFailureResultAsync();
        }

        if (string.IsNullOrEmpty(context.Activity.Text))
        {
            return GetUnsuccessfulTokenFailureResultAsync();
        }

        var matchedMagicCode = Regex.Match(input: context.Activity.Text, pattern: @"(\d{6})");
        if (matchedMagicCode.Success is false)
        {
            return GetUnsuccessfulTokenFailureResultAsync();
        }

        return context.GetUserTokenClientOrFailure(option).ForwardValueAsync(InnerGetUserTokenOrFailureAsync);

        async ValueTask<Result<TokenResponse, BotFlowFailure>> InnerGetUserTokenOrFailureAsync(UserTokenClient userTokenClient)
        {
            try
            {
                var userToken = await userTokenClient.GetUserTokenAsync(
                    userId: context.Activity.From.Id,
                    connectionName: option.OAuthConnectionName,
                    channelId: context.Activity.ChannelId,
                    magicCode: matchedMagicCode.Value,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                if (userToken is null)
                {
                    return new BotFlowFailure(option.UnsuccessfulTokenFailureMessage, "User token is null");
                }

                return userToken;
            }
            catch (Exception ex)
            {
                context.GetLogger().LogError(ex, "An unexpected exception was thrown by userTokenProvider.GetUserTokenAsync");
                return new BotFlowFailure(
                    userMessage: option.UnexpectedFailureMessage,
                    logMessage: $"An unexpected exception {ex.GetType().FullName} was thrown: {ex.Message}");
            }
        }

        ValueTask<Result<TokenResponse, BotFlowFailure>> GetUnsuccessfulTokenFailureResultAsync()
            =>
            new(new BotFlowFailure(option.UnsuccessfulTokenFailureMessage));
    }
}