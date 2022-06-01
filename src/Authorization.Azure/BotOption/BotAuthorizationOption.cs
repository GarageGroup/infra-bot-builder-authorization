using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class BotAuthorizationOption
{
    private const string DefaultUnsuccessfulTokenFailureMessage = "Не удалось авторизоваться. Повторите попытку";

    private const string DefaultUnexpectedFailureMessage
        =
        "Возникла непредвиденная ошибка при попытке получить данные пользователя. Повторите попытку позже или обратитесь к администратору";

    private const string DefaultEnterButtonTitle = "Вход";

    private const string DefaultEnterText = "Войдите в свою учетную запись";

    public BotAuthorizationOption(
        string oAuthConnectionName,
        [AllowNull] string enterButtonTitle = DefaultEnterButtonTitle,
        [AllowNull] string enterText = DefaultEnterText,
        [AllowNull] string unsuccessfulTokenFailureMessage = DefaultUnsuccessfulTokenFailureMessage,
        [AllowNull] string unexpectedFailureMessage = DefaultUnexpectedFailureMessage,
        [AllowNull] Func<BotUser, string>? successMessageFactory = null)
    {
        OAuthConnectionName = oAuthConnectionName;

        EnterButtonTitle = string.IsNullOrEmpty(enterButtonTitle) ? DefaultEnterButtonTitle: enterButtonTitle;
        EnterText = string.IsNullOrEmpty(enterText) ? DefaultEnterText : enterText;

        UnsuccessfulTokenFailureMessage = string.IsNullOrEmpty(unsuccessfulTokenFailureMessage)
            ? DefaultUnsuccessfulTokenFailureMessage : unsuccessfulTokenFailureMessage;

        UnexpectedFailureMessage = string.IsNullOrEmpty(unexpectedFailureMessage)
            ? DefaultUnexpectedFailureMessage : unexpectedFailureMessage;

        SuccessMessageFactory = successMessageFactory ?? CreateDefaultSuccessMessage;
    }

    public string OAuthConnectionName { get; }

    public string EnterButtonTitle { get; }

    public string EnterText { get; }

    public string UnsuccessfulTokenFailureMessage { get; }

    public string UnexpectedFailureMessage { get; }

    public Func<BotUser, string> SuccessMessageFactory { get; }

    private static string CreateDefaultSuccessMessage(BotUser botUser)
        =>
        $"{botUser.DisplayName}, авторизация прошла успешно!";
}