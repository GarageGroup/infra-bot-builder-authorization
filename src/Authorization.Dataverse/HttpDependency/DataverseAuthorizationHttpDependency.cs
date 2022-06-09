using System;
using System.Net.Http;
using PrimeFuncPack;

namespace GGroupp.Infra.Bot.Builder;

public static class DataverseAuthorizationHttpDependency
{
    public static Dependency<HttpMessageHandler> UseDataverseImpersonation(this Dependency<HttpMessageHandler> dependency, IBotContext botContext)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _ = botContext ?? throw new ArgumentNullException(nameof(botContext));

        return dependency.UseDataverseImpersonation(CreateCallerIdProvider);

        IAsyncValueFunc<Guid> CreateCallerIdProvider(IServiceProvider _)
            =>
            CallerIdProvider.InternalCreate(botContext.BotUserProvider);
    }
}