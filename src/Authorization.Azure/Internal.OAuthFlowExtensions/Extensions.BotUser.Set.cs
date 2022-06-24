using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static async Task SetBotUserAsync(this IBotContext botContext, BotUser botUser, CancellationToken cancellationToken)
    {
        _ = await botContext.BotUserProvider.SetCurrentUserAsync(botUser, cancellationToken).ConfigureAwait(false);

        var turnContext = botContext.TurnContext;
        var conversation = turnContext.Activity?.GetConversationReference();

        await botContext.CreateConversationReferenceAccessor().SetAsync(turnContext, conversation, cancellationToken).ConfigureAwait(false);
    }
}
