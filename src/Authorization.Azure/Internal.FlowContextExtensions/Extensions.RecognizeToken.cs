using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowContextExtensions
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

        return context.GetUserTokenPrividerOrFailure(option).ForwardValueAsync(InnerGetUserTokenOrFailureAsync);

        async ValueTask<Result<TokenResponse, BotFlowFailure>> InnerGetUserTokenOrFailureAsync(IExtendedUserTokenProvider userTokenProvider)
        {
            try
            {
                var userToken = await userTokenProvider.GetUserTokenAsync(
                    turnContext: context,
                    connectionName: option.OAuthConnectionName,
                    magicCode: matchedMagicCode.Value,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                if (userToken is null)
                {
                    return new BotFlowFailure(option.UnsuccessfulTokenFailureMessage);
                }

                return userToken;
            }
            catch (Exception ex)
            {
                context.GetLogger().LogError(ex, "An unexpected exception was thrown by userTokenProvider.GetUserTokenAsync");
                return new BotFlowFailure(option.UnexpectedFailureMessage);
            }
        }

        ValueTask<Result<TokenResponse, BotFlowFailure>> GetUnsuccessfulTokenFailureResultAsync()
            =>
            new(new BotFlowFailure(option.UnsuccessfulTokenFailureMessage));
    }
}