using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;

namespace GGroupp.Infra.Bot.Builder;

partial class TurnContextExtensions
{
    internal static bool IsEmulatorChannel(this ITurnContext turnContext)
        =>
        turnContext.Activity.ChannelId.EqualsInvariant(Channels.Emulator);
}