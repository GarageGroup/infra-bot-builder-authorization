namespace GGroupp.Infra.Bot.Builder;

public sealed record class BotAuthorizationOption
{
    public BotAuthorizationOption(string oAuthConnectionName)
        =>
        OAuthConnectionName = oAuthConnectionName ?? string.Empty;

    public string OAuthConnectionName { get; }
}