using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

internal interface IOAuthFlowContext : ITurnContext
{
    ILogger GetLogger();
}