using System;
using GGroupp.Infra;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

public static class DataverseAuthorizationBotBuilder
{
    public static IBotBuilder UseDataverseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserMeGetFunc> azureUserGetFuncResolver,
        Func<IBotContext, IDataverseUserGetSupplier> dataverseUserApiResolver)
    {
        ArgumentNullException.ThrowIfNull(botBuilder);
        ArgumentNullException.ThrowIfNull(optionResolver);
        ArgumentNullException.ThrowIfNull(azureUserGetFuncResolver);
        ArgumentNullException.ThrowIfNull(dataverseUserApiResolver);

        return botBuilder.UseAuthorization(optionResolver, azureUserGetFuncResolver, InnerResolveBotUserGetFunc);

        IBotUserGetFunc InnerResolveBotUserGetFunc(IBotContext botContext)
            =>
            new BotDataverseUserGetFunc(
                dataverseUserApi: dataverseUserApiResolver.Invoke(botContext));
    }
}