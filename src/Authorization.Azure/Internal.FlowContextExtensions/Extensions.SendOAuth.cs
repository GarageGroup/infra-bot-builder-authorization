using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowContextExtensions
{
    internal static ValueTask<Result<ResourceResponse, BotFlowFailure>> SendOAuthCardOrFailureAsync(
        this IOAuthFlowContext context, BotAuthorizationOption option, CancellationToken cancellationToken)
    {
        if (context.IsChannelNotSupported())
        {
            var notSupportedChannelFailure = new BotFlowFailure(option.UnexpectedFailureMessage);
            return new(notSupportedChannelFailure);
        }

        return context.GetUserTokenPrividerOrFailure(option).ForwardValueAsync(InnerSendActivityAsync);

        async ValueTask<Result<ResourceResponse, BotFlowFailure>> InnerSendActivityAsync(IExtendedUserTokenProvider userTokenProvider)
        {
            try
            {
                var signInResource = await userTokenProvider.GetSignInResourceAsync(
                    turnContext: context,
                    connectionName: option.OAuthConnectionName,
                    userId: context.Activity.From.Id,
                    finalRedirect: default,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                var activity = CreateOAuthCard(context, signInResource, option).ToActivity(inputHint: InputHints.AcceptingInput);
                return await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.GetLogger().LogError(ex, "An unexpected exception was thrown by UserTokenClient.GetSignInResourceAsync");
                return new BotFlowFailure(option.UnexpectedFailureMessage);
            }
        }
    }

    private static Attachment CreateOAuthCard(ITurnContext turnContext, SignInResource signInResource, BotAuthorizationOption option)
        =>
        new()
        {
            ContentType = OAuthCard.ContentType,
            Content = new OAuthCard
            {
                Text = option.EnterText,
                ConnectionName = option.OAuthConnectionName,
                Buttons = new[]
                {
                    new CardAction
                    {
                        Title = option.EnterButtonTitle,
                        Text = option.EnterText,
                        Type = turnContext.IsEmulatorChannel() ? ActionTypes.OpenUrl : ActionTypes.Signin,
                        Value = signInResource.SignInLink
                    }
                },
                TokenExchangeResource = signInResource.TokenExchangeResource,
            }
        };
}