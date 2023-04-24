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
        ArgumentNullException.ThrowIfNull(botContext);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Unit>(cancellationToken);
        }

        return InnerInvokeAsync(botContext, cancellationToken);
    }

    private async ValueTask<Unit> InnerInvokeAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        if (await IsUserAlreadyAuthorizedAsync(botContext.BotUserProvider, cancellationToken).ConfigureAwait(false))
        {
            return await botContext.BotFlow.NextAsync(cancellationToken).ConfigureAwait(false);
        }

        return botContext.TurnContext.IsMsteamsChannel() switch
        {
            true => await AuthorizeInTeamsAsync(botContext, cancellationToken).ConfigureAwait(false),
            _ => await AuthorizeNotTeamsAsync(botContext, cancellationToken).ConfigureAwait(false)
        };
    }

    private static async ValueTask<bool> IsUserAlreadyAuthorizedAsync(IBotUserProvider botUserProvider, CancellationToken cancellationToken)
        =>
        await botUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false) is not null;

    private async ValueTask<Unit> AuthorizeInTeamsAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var flowContext = CreateFlowContext(botContext);
        botContext.BotTelemetryClient.TrackEvent(FlowId, flowContext.Activity.Id, "TeamsStart");

        var teamsResult = await flowContext.AuthorizeInTeamsAsync(botUserGetFunc, option, cancellationToken).ConfigureAwait(false);

        return await teamsResult.FoldValueAsync(NextForTeamsAsync, InnerOnFailureAsync).ConfigureAwait(false);

        async ValueTask<Unit> NextForTeamsAsync(BotUser botUser)
        {
            await botContext.SetBotUserAsync(botUser, cancellationToken).ConfigureAwait(false);

            botContext.BotTelemetryClient.TrackEvent(FlowId, flowContext.Activity.Id, "TeamsComplete");
            return await botContext.BotFlow.NextAsync(cancellationToken).ConfigureAwait(false);
        }

        async ValueTask<Unit> InnerOnFailureAsync(BotFlowFailure failure)
        {
            var unit = await OnFailureAsync(flowContext, failure, cancellationToken).ConfigureAwait(false);

            botContext.BotTelemetryClient.TrackEvent(FlowId, flowContext.Activity.Id, "TeamsBreak", failure.LogMessage);
            return unit;
        }
    }

    private async ValueTask<Unit> AuthorizeNotTeamsAsync(IBotContext botContext, CancellationToken cancellationToken)
    {
        var flowContext = CreateFlowContext(botContext);

        var sourceActivityAccessor = botContext.ConversationState.CreateProperty<Activity?>("__authSourceActivity");
        var oAuthCardResourceAccessor = botContext.ConversationState.CreateProperty<ResourceResponse?>("__authCardResource");

        var sourceActivity = await sourceActivityAccessor.GetAsync(flowContext, default, cancellationToken).ConfigureAwait(false);
        if (sourceActivity is not null)
        {
            var tokenResult = await flowContext.RecognizeTokenOrFailureAsync(option, cancellationToken).ConfigureAwait(false);
            var sendFailureResult = await tokenResult.MapFailureValueAsync(InnerOnFailureAsync).ConfigureAwait(false);

            return await sendFailureResult.FoldValueAsync(AzureAuthAsync, SendOAuthCardOrBreakAsync).ConfigureAwait(false);
        }

        botContext.BotTelemetryClient.TrackEvent(FlowId, flowContext.Activity.Id, "Start");

        await sourceActivityAccessor.SetAsync(flowContext, flowContext.Activity, cancellationToken).ConfigureAwait(false);
        return await SendOAuthCardOrBreakAsync(default).ConfigureAwait(false);

        async ValueTask<Unit> AzureAuthAsync(TokenResponse tokenResponse)
        {
            var azureResult = await tokenResponse.AuthorizeInAzureAsync(
                azureUserGetFunc, botUserGetFunc, option, cancellationToken).ConfigureAwait(false);

            return await azureResult.FoldValueAsync(NextAsync, BreakAsync).ConfigureAwait(false);
        }

        async ValueTask<Unit> NextAsync(BotUser botUser)
        {
            var activity = MessageFactory.Text(option.SuccessMessageFactory.Invoke(botUser));
            var successActivityTask = flowContext.SendActivityAsync(activity, cancellationToken);

            var replaceOAuthCardTask = ReplaceOAuthCardResourceAsync(default).AsTask();
            await Task.WhenAll(successActivityTask, replaceOAuthCardTask).ConfigureAwait(false);

            var setCurrentUserTask = botContext.SetBotUserAsync(botUser, cancellationToken);
            var clearCacheTask = ClearCacheAsync();

            await Task.WhenAll(setCurrentUserTask, clearCacheTask).ConfigureAwait(false);

            botContext.BotTelemetryClient.TrackEvent(FlowId, sourceActivity.Id, "Complete");
            return await botContext.BotFlow.NextAsync(sourceActivity, cancellationToken).ConfigureAwait(false);
        }

        async ValueTask<Unit> SendOAuthCardOrBreakAsync(Unit _)
        {
            var sendResult = await flowContext.SendOAuthCardOrFailureAsync(option, cancellationToken).ConfigureAwait(false);
            return await sendResult.FoldValueAsync(SaveOAuthCardResourceAsync, BreakAsync).ConfigureAwait(false);
        }

        async ValueTask<Unit> BreakAsync(BotFlowFailure flowFailure)
        {
            var unit = await ReplaceOAuthCardResourceAsync(default).ConfigureAwait(false);

            var clearCacheTask = ClearCacheAsync();
            var onFailureTask = InnerOnFailureAsync(flowFailure).AsTask();

            await Task.WhenAll(clearCacheTask, onFailureTask).ConfigureAwait(false);

            botContext.BotTelemetryClient.TrackEvent(FlowId, sourceActivity?.Id ?? flowContext.Activity.Id, "Break", flowFailure.LogMessage);
            return unit;
        }

        async ValueTask<Unit> SaveOAuthCardResourceAsync(ResourceResponse? oAuthCardResource)
        {
            var unit = await ReplaceOAuthCardResourceAsync(default).ConfigureAwait(false);
            await oAuthCardResourceAccessor.SetAsync(flowContext, oAuthCardResource, cancellationToken).ConfigureAwait(false);
            return unit;
        }

        async ValueTask<Unit> ReplaceOAuthCardResourceAsync(Unit unit)
        {
            if (flowContext.IsNotTelegramChannel())
            {
                return unit;
            }

            var resource = await oAuthCardResourceAccessor.GetAsync(flowContext, null, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(resource?.Id))
            {
                return unit;
            }

            var oAuthCardMessage = MessageFactory.Text(option.EnterText);
            oAuthCardMessage.Id = resource.Id;

            _ = await flowContext.UpdateActivityAsync(oAuthCardMessage, cancellationToken).ConfigureAwait(false);
            return unit;
        }

        Task ClearCacheAsync()
        {
            var clearOAuthCardCacheTask = oAuthCardResourceAccessor.DeleteAsync(flowContext, cancellationToken);
            var clearSourceActivityCacheTask = sourceActivityAccessor.DeleteAsync(flowContext, cancellationToken);

            return Task.WhenAll(clearOAuthCardCacheTask, clearSourceActivityCacheTask);
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