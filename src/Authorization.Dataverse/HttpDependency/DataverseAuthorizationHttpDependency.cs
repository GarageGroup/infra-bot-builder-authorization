using System;
using System.Net.Http;
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
        dependency.UseDataverseImpersonation(
            _ => CallerIdProvider.InternalCreate(botContext.BotUserProvider));
}