using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class OAuthFlowExtensions
{
    private static bool IsChannelNotSupported(this ITurnContext turnContext)
        =>
        string.Equals(turnContext.Activity.ChannelId, Channels.Skype, StringComparison.InvariantCultureIgnoreCase);

    private static Result<UserTokenClient, BotFlowFailure> GetUserTokenClientOrFailure(
        this IOAuthFlowContext context, BotAuthorizationOption option)
    {
        var userTokenClient = context.TurnState.Get<UserTokenClient>();
        if (userTokenClient is not null)
        {
            return Result.Success(userTokenClient);
        }

        return new BotFlowFailure(
            userMessage: option.UnexpectedFailureMessage,
            logMessage: "UserTokenClient must be specified in the turn state");
    }

    private static IStatePropertyAccessor<ConversationReference?> CreateConversationReferenceAccessor(this IBotContext botContext)
        =>
        botContext.UserState.CreateProperty<ConversationReference?>("__conversationReference");
}