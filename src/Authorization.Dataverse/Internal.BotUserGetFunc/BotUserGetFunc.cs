using System;
using GGroupp.Infra;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

internal sealed partial class BotDataverseUserGetFunc : IBotUserGetFunc
{
    private const string UserNotFoundFailureMessage
        =
        "User not found. You may not have the rights to access the system";

    private const string UnexpectedFailureMessage
        =
        "An unexpected error occurred when trying to access Dataverse. Please try again later or contact the administrator";

    private readonly IDataverseUserGetSupplier dataverseUserApi;

    internal BotDataverseUserGetFunc(IDataverseUserGetSupplier dataverseUserApi)
        =>
        this.dataverseUserApi = dataverseUserApi;
}