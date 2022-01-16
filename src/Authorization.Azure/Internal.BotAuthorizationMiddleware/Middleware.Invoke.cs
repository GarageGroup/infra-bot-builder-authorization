using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class BotAuthorizationMiddleware
{
    public ValueTask<Unit> InvokeAsync(IBotContext botContext, CancellationToken cancellationToken = default)
    {
        _ = botContext ?? throw new ArgumentNullException(nameof(botContext));

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Unit>(cancellationToken);
        }

        return InnerInvokeAsync(botContext, cancellationToken);
    }

    private async ValueTask<Unit> InnerInvokeAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        if (await IsAlreadyAuthorizedAsync(botContext.BotUserProvider, cancellationToken).ConfigureAwait(false))
        {
            return await botContext.BotFlow.NextAsync(cancellationToken).ConfigureAwait(false);
        }

        return botContext.TurnContext.IsTeamsChannel() switch
        {
            true => await AuthorizeInTeamsAsync(botContext, cancellationToken).ConfigureAwait(false),
            _ => await AuthorizeNotTeamsAsync(botContext, cancellationToken).ConfigureAwait(false)
        };
    }

    private static async ValueTask<bool> IsAlreadyAuthorizedAsync(IBotUserProvider botUserProvider, CancellationToken cancellationToken)
    {
        var currentUser = await botUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        return currentUser is not null;
    }

    private async ValueTask<Unit> AuthorizeInTeamsAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var flowContext = CreateFlowContext(botContext);
        var teamsResult = await flowContext.AuthorizeInTeamsAsync(botUserGetFunc, cancellationToken).ConfigureAwait(false);

        return await teamsResult.FoldValueAsync(NextForTeamsAsync, InnerOnFailureAsync).ConfigureAwait(false);

        async ValueTask<Unit> NextForTeamsAsync(BotUser botUser)
        {
            _ = await botContext.BotUserProvider.SetCurrentUserAsync(botUser, cancellationToken).ConfigureAwait(false);
            return await botContext.BotFlow.NextAsync(cancellationToken).ConfigureAwait(false);
        }

        ValueTask<Unit> InnerOnFailureAsync(BotFlowFailure failure)
            =>
            OnFailureAsync(flowContext, failure, cancellationToken);
    }

    private async ValueTask<Unit> AuthorizeNotTeamsAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var flowContext = CreateFlowContext(botContext);
        var sourceActivityAccessor = botContext.UserState.CreateProperty<Activity?>("__authSourceActivity");

        var sourceActivity = await sourceActivityAccessor.GetAsync(flowContext, default, cancellationToken).ConfigureAwait(false);
        if (sourceActivity is not null)
        {
            var tokenResult = await flowContext.RecognizeTokenOrFailureAsync(option.OAuthConnectionName, cancellationToken).ConfigureAwait(false);
            var sendFailureResult = await tokenResult.MapFailureValueAsync(InnerOnFailureAsync).ConfigureAwait(false);

            return await sendFailureResult.FoldValueAsync(AzureAuthAsync, SendOAuthCardOrBreakAsync).ConfigureAwait(false);
        }

        await sourceActivityAccessor.SetAsync(flowContext, flowContext.Activity, cancellationToken).ConfigureAwait(false);
        return await SendOAuthCardOrBreakAsync(default).ConfigureAwait(false);

        async ValueTask<Unit> AzureAuthAsync(TokenResponse tokenResponse)
        {
            var azureResult = await flowContext.AuthorizeInAzureAsync(
                azureUserGetFunc, botUserGetFunc, tokenResponse, cancellationToken).ConfigureAwait(false);

            return await azureResult.FoldValueAsync(NextAsync, BreakAsync).ConfigureAwait(false);
        }

        async ValueTask<Unit> NextAsync(BotUser botUser)
        {
            var activity = MessageFactory.Text($"{botUser.DisplayName}, авторизация прошла успешно!");
            _ = await flowContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            _ = await botContext.BotUserProvider.SetCurrentUserAsync(botUser, cancellationToken).ConfigureAwait(false);

            await sourceActivityAccessor.DeleteAsync(flowContext, cancellationToken).ConfigureAwait(false);
            return await botContext.BotFlow.NextAsync(sourceActivity, cancellationToken).ConfigureAwait(false);
        }

        async ValueTask<Unit> SendOAuthCardOrBreakAsync(Unit _)
        {
            var sendResult = await flowContext.SendOAuthCardOrFailureAsync(option.OAuthConnectionName, cancellationToken).ConfigureAwait(false);
            return await sendResult.FoldValueAsync(ValueTask.FromResult, BreakAsync).ConfigureAwait(false);
        }

        async ValueTask<Unit> BreakAsync(BotFlowFailure flowFailure)
        {
            await sourceActivityAccessor.DeleteAsync(flowContext, cancellationToken).ConfigureAwait(false);
            return await InnerOnFailureAsync(flowFailure).ConfigureAwait(false);
        }

        ValueTask<Unit> InnerOnFailureAsync(BotFlowFailure failure)
            =>
            OnFailureAsync(flowContext, failure, cancellationToken);
    }

    private static async ValueTask<Unit> OnFailureAsync(IOAuthFlowContext flowContext, BotFlowFailure failure, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(failure.UserMessage) is false)
        {
            var failureActivity = MessageFactory.Text(failure.UserMessage);
            _ = await flowContext.SendActivityAsync(failureActivity, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(failure.LogMessage) is false)
        {
            flowContext.GetLogger().LogError("{logMessage}", failure.LogMessage);
        }

        return default;
    }
}