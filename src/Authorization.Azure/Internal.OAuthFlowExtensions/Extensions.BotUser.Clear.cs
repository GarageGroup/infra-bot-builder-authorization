using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static async Task ClearBotUserAsync(this IBotContext botContext, CancellationToken cancellationToken)
    {
        _ = await botContext.BotUserProvider.SetCurrentUserAsync(default, cancellationToken).ConfigureAwait(false);

        var turnContext = botContext.TurnContext;
        await botContext.CreateConversationReferenceAccessor().DeleteAsync(turnContext, cancellationToken).ConfigureAwait(false);
    }
}