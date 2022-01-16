using System.Collections.Generic;
using System.Globalization;
using GGroupp.Platform;

namespace GGroupp.Infra.Bot.Builder;

partial class DataverseUserExtensions
{
    internal static BotUser ToUserDataJson(this DataverseUserGetOut dataverseUser, AzureUserGetOut azureUser)
        =>
        new(
            id: azureUser.Id,
            mail: azureUser.Mail,
            displayName: azureUser.DisplayName,
            claims: new Dictionary<string, string>
            {
                [DataverseUserIdClaimName] = dataverseUser.SystemUserId.ToString("D", CultureInfo.InvariantCulture),
                [DataverseUserFirstNameClaimName] = dataverseUser.FirstName,
                [DataverseUserLastNameClaimName] = dataverseUser.LastName,
                [DataverseUserFullNameClaimName] = dataverseUser.FullName
            });
}