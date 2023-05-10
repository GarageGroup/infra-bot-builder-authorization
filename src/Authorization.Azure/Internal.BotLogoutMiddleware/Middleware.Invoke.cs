using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class BotLogoutMiddleware
{
    public ValueTask<Unit> InvokeAsync(IBotContext botContext, CancellationToken cancellationToken = default)
    {
        _ = botContext ?? throw new ArgumentNullException(nameof(botContext));

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Unit>(cancellationToken);
        }

        return botContext.TurnContext.IsMsteamsChannel() switch
        {
            true => InnerNextAsync(),
            _ => botContext.TurnContext.RecognizeCommandOrAbsent(commandName).FoldValueAsync(InnerLogoutAsync, InnerNextAsync)
        };

        ValueTask<Unit> InnerLogoutAsync(string _)
            =>
            LogoutAsync(botContext, cancellationToken);

        ValueTask<Unit> InnerNextAsync()
            =>
            botContext.BotFlow.NextAsync(cancellationToken);
    }

    private async ValueTask<Unit> LogoutAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var user = await botContext.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            var activity = MessageFactory.Text(logoutOption.UserNotAuthorizedMessage);
            _ = await botContext.TurnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            return default;
        }

        await ClearAuthDataAsync(botContext, cancellationToken).ConfigureAwait(false);

        var successActivity = MessageFactory.Text(logoutOption.SuccessMessage);
        if (botContext.TurnContext.IsTelegramChannel())
        {
            successActivity.ChannelData = CreateTelegramChannelData();
        }

        _ = await botContext.TurnContext.SendActivityAsync(successActivity, cancellationToken).ConfigureAwait(false);
        return default;
    }

    private static Task ClearAuthDataAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var botUserClearTask = botContext.ClearBotUserAsync(cancellationToken);
        var conversationStateClearTask = botContext.ConversationState.ClearStateAsync(botContext.TurnContext, cancellationToken);

        return Task.WhenAll(botUserClearTask, conversationStateClearTask);
    }

    private static JObject CreateTelegramChannelData()
        =>
        new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardRemove()
            })
        .ToJObject();
}