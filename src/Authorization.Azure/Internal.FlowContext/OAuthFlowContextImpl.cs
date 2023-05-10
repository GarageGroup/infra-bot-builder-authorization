using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed class OAuthFlowContextImpl : IOAuthFlowContext
{
    private readonly ITurnContext turnContext;

    private readonly ILogger logger;

    internal OAuthFlowContextImpl(ITurnContext turnContext, ILogger logger)
    {
        this.turnContext = turnContext;
        this.logger = logger;
    }

    public BotAdapter Adapter => turnContext.Adapter;

    public TurnContextStateCollection TurnState => turnContext.TurnState;

    public Activity Activity => turnContext.Activity;

    public bool Responded => turnContext.Responded;

    public ILogger GetLogger() => logger;

    public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default)
        =>
        turnContext.DeleteActivityAsync(activityId, cancellationToken);

    public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default)
        =>
        turnContext.DeleteActivityAsync(conversationReference, cancellationToken);

    public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        =>
        turnContext.OnDeleteActivity(handler);

    public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        =>
        turnContext.OnSendActivities(handler);

    public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        =>
        turnContext.OnUpdateActivity(handler);

    public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default)
        =>
        turnContext.SendActivitiesAsync(activities, cancellationToken);

    public Task<ResourceResponse> SendActivityAsync(
        string textReplyToSend,
        string? speak = null,
        string inputHint = "acceptingInput",
        CancellationToken cancellationToken = default)
        =>
        turnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);

    public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        =>
        turnContext.SendActivityAsync(activity, cancellationToken);

    public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        =>
        turnContext.UpdateActivityAsync(activity, cancellationToken);
}