using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static void TrackEvent(this IBotTelemetryClient client, string flowId, string instanceId, string eventName, string? eventMessage = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "FlowId", flowId },
            { "InstanceId", instanceId },
        };

        if (string.IsNullOrEmpty(eventMessage) is false)
        {
            properties.Add("Message", eventMessage);
        }

        client.TrackEvent(flowId + eventName, properties);
    }
}