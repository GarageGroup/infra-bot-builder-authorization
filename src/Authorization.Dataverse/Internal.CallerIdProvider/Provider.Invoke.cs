using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CallerIdProvider
{
    public ValueTask<Guid> InvokeAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Guid>(cancellationToken);
        }

        return InnerGetUserIdAsync(cancellationToken);
    }

    private async ValueTask<Guid> InnerGetUserIdAsync(CancellationToken cancellationToken)
    {
        var botUser = await botUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        return botUser?.GetDataverseUserIdOrAbsent().OrDefault() ?? default;
    }
}