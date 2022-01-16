using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowContextExtensions
{
    internal static ValueTask<Result<Unit, BotFlowFailure>> SendOAuthCardOrFailureAsync(
        this IOAuthFlowContext context, string connectionName, CancellationToken cancellationToken)
    {
        if (context.IsChannelNotSupported())
        {
            var notSupportedChannelFailure = new BotFlowFailure(UnexpectedFailureMessage);
            return new(notSupportedChannelFailure);
        }

        return context.GetUserTokenPrividerOrFailure().ForwardValueAsync(InnerSendActivityAsync);

        async ValueTask<Result<Unit, BotFlowFailure>> InnerSendActivityAsync(
            IExtendedUserTokenProvider userTokenProvider)
        {
            try
            {
                var signInResource = await userTokenProvider.GetSignInResourceAsync(
                    turnContext: context,
                    connectionName: connectionName,
                    userId: context.Activity.From.Id,
                    finalRedirect: default,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                var activity = CreateOAuthCard(context, signInResource, connectionName).ToActivity(inputHint: InputHints.AcceptingInput);
                _ = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

                return default(Unit);
            }
            catch (Exception ex)
            {
                context.GetLogger().LogError(ex, "An unexpected exception was thrown by UserTokenClient.GetSignInResourceAsync");
                return new BotFlowFailure(UnexpectedFailureMessage);
            }
        }
    }

    private static Attachment CreateOAuthCard(ITurnContext turnContext, SignInResource signInResource, string connectionName)
        =>
        new()
        {
            ContentType = OAuthCard.ContentType,
            Content = new OAuthCard
            {
                Text = EnterText,
                ConnectionName = connectionName,
                Buttons = new[]
                {
                    new CardAction
                    {
                        Title = EnterButtonTitle,
                        Text = EnterText,
                        Type = turnContext.IsEmulatorChannel() ? ActionTypes.OpenUrl : ActionTypes.Signin,
                        Value = signInResource.SignInLink
                    }
                },
                TokenExchangeResource = signInResource.TokenExchangeResource,
            }
        };
}