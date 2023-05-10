using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace GarageGroup.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static void TrackEvent(this IBotTelemetryClient client, string flowId, string instanceId, string eventName, string eventMessage)
    {
        var properties = new Dictionary<string, string>
        {
            ["flowId"] = flowId,
            ["instanceId"] = instanceId,
            ["event"] = eventName,
            ["message"] = eventMessage
        };

        client.TrackEvent(flowId + eventName, properties);
    }
}