using System;
using GGroupp.Infra;

namespace GarageGroup.Infra.Bot.Builder;

using IBotUserGetFunc = IAsyncValueFunc<AzureUserGetOut, Result<BotUser, BotFlowFailure>>;

internal sealed partial class BotUserGetFunc : IBotUserGetFunc
{
    public static BotUserGetFunc Instance { get; }

    static BotUserGetFunc()
        =>
        Instance = new();

    private BotUserGetFunc()
    {
    }
}