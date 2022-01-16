using System;
using System.Linq;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class TurnContextExtensions
{
    internal static bool IsChannelNotSupported(this ITurnContext turnContext)
        =>
        notSupportedChannles.Contains(turnContext.Activity.ChannelId, StringComparer.InvariantCultureIgnoreCase);
}