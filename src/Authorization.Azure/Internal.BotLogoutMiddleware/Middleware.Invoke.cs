using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class BotLogoutMiddleware
{
    public ValueTask<Unit> InvokeAsync(IBotContext botContext, CancellationToken cancellationToken = default)
    {
        _ = botContext ?? throw new ArgumentNullException(nameof(botContext));

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Unit>(cancellationToken);
        }

        return botContext.TurnContext.IsTeamsChannel() switch
        {
            true => InnerNextAsync(),
            _ => botContext.TurnContext.Activity.RecognizeCommandOrAbsent(commandName).FoldValueAsync(InnerLogoutAsync, InnerNextAsync)
        };

        ValueTask<Unit> InnerLogoutAsync(string _)
            =>
            LogoutAsync(botContext, cancellationToken);

        ValueTask<Unit> InnerNextAsync()
            =>
            botContext.BotFlow.NextAsync(cancellationToken);
    }

    private static async ValueTask<Unit> LogoutAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var user = await botContext.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            var activity = MessageFactory.Text("Вы не авторизованы");
            _ = await botContext.TurnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return default;
        }

        _ = await botContext.BotUserProvider.SetCurrentUserAsync(default, cancellationToken).ConfigureAwait(false);

        await botContext.UserState.ClearStateAsync(botContext.TurnContext, cancellationToken).ConfigureAwait(false);
        await botContext.ConversationState.ClearStateAsync(botContext.TurnContext, cancellationToken).ConfigureAwait(false);

        var successActivity = MessageFactory.Text("Вы вышли из учетной записи");
        if (botContext.TurnContext.Activity.IsTelegram())
        {
            successActivity = successActivity.SetReplyTelegramKeyboardRemoveChannelData();
        }

        _ = await botContext.TurnContext.SendActivityAsync(successActivity, cancellationToken).ConfigureAwait(false);
        return default;
    }
}