using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;

namespace GGroupp.Infra.Bot.Builder;

partial class TurnContextExtensions
{
    internal static bool IsTeamsChannel(this ITurnContext context)
        =>
        context.Activity.ChannelId.EqualsInvariant(Channels.Msteams);
}