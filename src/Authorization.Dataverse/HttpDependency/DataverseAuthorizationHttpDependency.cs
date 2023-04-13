using System;
using System.Net.Http;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class DataverseAuthorizationHttpDependency
{
    public static Dependency<HttpMessageHandler> UseDataverseImpersonation(this Dependency<HttpMessageHandler> dependency, IBotContext botContext)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(botContext);

        return dependency.UseDataverseImpersonation(CreateCallerIdProvider);

        IAsyncValueFunc<Guid> CreateCallerIdProvider(IServiceProvider _)
            =>
            CallerIdProvider.InternalCreate(botContext.BotUserProvider);
    }
}