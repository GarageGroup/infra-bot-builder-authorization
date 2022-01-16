using System;
using GGroupp.Platform;

namespace GGroupp.Infra.Bot.Builder;

using IAzureUserGetFunc = IAsyncValueFunc<AzureUserMeGetIn, Result<AzureUserGetOut, Failure<AzureUserGetFailureCode>>>;
using IDataverseUserGetFunc = IAsyncValueFunc<DataverseUserGetIn, Result<DataverseUserGetOut, Failure<DataverseUserGetFailureCode>>>;
using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

public static class DataverseAuthorizationBotBuilder
{
    public static IBotBuilder UseDataverseAuthorization(
        this IBotBuilder botBuilder,
        Func<IBotContext, BotAuthorizationOption> optionResolver,
        Func<IBotContext, IAzureUserGetFunc> azureUserGetFuncResolver,
        Func<IBotContext, IDataverseUserGetFunc> dataverseUserGetFuncResolver)
    {
        _ = botBuilder ?? throw new ArgumentNullException(nameof(botBuilder));
        _ = optionResolver ?? throw new ArgumentNullException(nameof(optionResolver));
        _ = azureUserGetFuncResolver ?? throw new ArgumentNullException(nameof(azureUserGetFuncResolver));
        _ = dataverseUserGetFuncResolver ?? throw new ArgumentNullException(nameof(dataverseUserGetFuncResolver));

        return botBuilder.UseAuthorization(optionResolver, azureUserGetFuncResolver, InnerResolveBotUserGetFunc);

        IBotUserGetFunc InnerResolveBotUserGetFunc(IBotContext botContext)
            =>
            new BotDataverseUserGetFunc(
                dataverseUserGetFunc: dataverseUserGetFuncResolver.Invoke(botContext));
    }
}