using System;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

internal static partial class OAuthFlowContextExtensions
{
    private const string UnsuccessfulTokenFailureMessage
        =
        "Не удалось авторизоваться. Повторите попытку";

    private const string UnexpectedFailureMessage
        =
        "Возникла непредвиденная ошибка при попытке получить данные пользователя. Повторите попытку позже или обратитесь к администратору";

    private const string EnterButtonTitle = "Вход";

    private const string EnterText = "Войдите в свою учетную запись";

    private static Result<IExtendedUserTokenProvider, BotFlowFailure> GetUserTokenPrividerOrFailure(this IOAuthFlowContext context)
    {
        if (context.Adapter is IExtendedUserTokenProvider adapter)
        {
            return Result.Success(adapter);
        }

        return new BotFlowFailure(
            userMessage: UnexpectedFailureMessage,
            logMessage: "UserTokenClient must be specified in the turn state");
    }
}