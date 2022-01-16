using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace GGroupp.Infra.Bot.Builder;

internal static partial class TurnContextExtensions
{
    private static readonly IReadOnlyCollection<string> notSupportedChannles;

    static TurnContextExtensions()
        =>
        notSupportedChannles = new[]
        {
            Channels.Cortana,
            Channels.Skype,
            Channels.Skypeforbusiness
        };

    private static bool EqualsInvariant(this string? a, string? b)
        =>
        string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
}