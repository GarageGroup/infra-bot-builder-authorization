using System;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed partial class CallerIdProvider : IAsyncValueFunc<Guid>
{
    internal static CallerIdProvider InternalCreate(IBotUserProvider botUserProvider)
        =>
        new(botUserProvider);

    private readonly IBotUserProvider botUserProvider;

    private CallerIdProvider(IBotUserProvider botUserProvider)
        =>
        this.botUserProvider = botUserProvider;
}