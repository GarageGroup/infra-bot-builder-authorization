using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class BotUserGetFunc
{
    public ValueTask<Result<BotUser, BotFlowFailure>> InvokeAsync(AzureUserGetOut azureUser, CancellationToken cancellationToken = default)
    {
        var botUser = new BotUser(
            id: azureUser.Id,
            mail: azureUser.Mail,
            displayName: azureUser.DisplayName);
        
        return new(botUser);
    }
}