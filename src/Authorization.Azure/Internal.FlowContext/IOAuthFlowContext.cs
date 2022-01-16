using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

internal interface IOAuthFlowContext : ITurnContext
{
    ILogger GetLogger();
}