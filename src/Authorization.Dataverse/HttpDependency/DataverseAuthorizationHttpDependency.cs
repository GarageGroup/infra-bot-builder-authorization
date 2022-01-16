using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class DataverseAuthorizationHttpDependency
{
    public static Dependency<DelegatingHandler> UseDataverseImpersonation<THttpHandler>(
        this Dependency<THttpHandler> dependency, IBotContext botContext)
        where THttpHandler : HttpMessageHandler
        =>
        InnerUseDataverseImpersonation(
            dependency ?? throw new ArgumentNullException(nameof(dependency)),
            botContext ?? throw new ArgumentNullException(nameof(botContext)));

    private static Dependency<DelegatingHandler> InnerUseDataverseImpersonation<THttpHandler>(
        Dependency<THttpHandler> dependency, IBotContext botContext)
        where THttpHandler : HttpMessageHandler
        =>
        dependency.UseCallerId(
            _ => AsyncValueFunc.From(botContext.GetUserIdAsync));

    private static async ValueTask<Guid> GetUserIdAsync(this IBotContext context, CancellationToken cancellationToken)
    {
        var botUser = await context.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        return botUser?.GetDataverseUserIdOrAbsent().OrDefault() ?? default;
    }
}