using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class BotLogoutOption
{
    private const string DefaultSuccessMessage = "Вы вышли из учетной записи";

    private const string DefaultUserNotAuthorizedMessage = "Вы не авторизованы";

    public BotLogoutOption(
        [AllowNull] string successMessage = DefaultSuccessMessage,
        [AllowNull] string userNotAuthorizedMessage = DefaultUserNotAuthorizedMessage)
    {
        SuccessMessage = string.IsNullOrEmpty(successMessage) ? DefaultSuccessMessage : successMessage;
        UserNotAuthorizedMessage = string.IsNullOrEmpty(userNotAuthorizedMessage) ? DefaultUserNotAuthorizedMessage : userNotAuthorizedMessage;
    }

    public string SuccessMessage { get; }

    public string UserNotAuthorizedMessage { get; }
}