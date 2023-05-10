using System;
using System.Collections.Generic;
using System.Globalization;
using GGroupp.Infra;

namespace GarageGroup.Infra.Bot.Builder;

partial class DataverseUserExtensions
{
    internal static BotUser ToUserDataJson(this DataverseUserGetOut dataverseUser, AzureUserGetOut azureUser)
        =>
        new(
            id: azureUser.Id,
            mail: azureUser.Mail,
            displayName: azureUser.DisplayName,
            claims: new FlatArray<KeyValuePair<string, string>>(
                new(DataverseUserIdClaimName, dataverseUser.SystemUserId.ToString("D", CultureInfo.InvariantCulture)),
                new(DataverseUserFirstNameClaimName, dataverseUser.FirstName),
                new(DataverseUserLastNameClaimName, dataverseUser.LastName),
                new(DataverseUserFullNameClaimName, dataverseUser.FullName)));
}