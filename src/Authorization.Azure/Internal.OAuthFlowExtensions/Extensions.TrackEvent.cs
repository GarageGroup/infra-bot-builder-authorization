using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace GarageGroup.Infra.Bot.Builder;

partial class OAuthFlowExtensions
{
    internal static void TrackEvent(
        this IBotTelemetryClient client, string flowId, string instanceId, string eventName, string eventMessage, Exception? sourceException)
    {
        var properties = new Dictionary<string, string>
        {
            ["flowId"] = flowId,
            ["instanceId"] = instanceId,
            ["event"] = eventName,
            ["message"] = eventMessage
        };

        if (sourceException is not null)
        {
            properties["errorMessage"] = sourceException.Message ?? string.Empty;
            properties["errorType"] = sourceException.GetType().FullName ?? string.Empty;
            properties["stackTrace"] = sourceException.StackTrace ?? string.Empty;
        }

        client.TrackEvent(flowId + eventName, properties);
    }
}