using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Bot.Api.Repositories.Common;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public abstract class GoogleSheetsRepositoryBase
{
    private SpreadsheetsResource Resource { get; }
    private string SpreadSheetId { get; }

    protected GoogleSheetsRepositoryBase(IVostokHostingEnvironment environment, string spreadSheetId)
    {
        SpreadSheetId = spreadSheetId;
        Resource = GetSpreadsheetsResource();

        SpreadsheetsResource GetSpreadsheetsResource()
        {
            var scopes = new[] { SheetsService.Scope.Spreadsheets };

            var credential = GoogleCredential
                .FromJson(environment.SecretConfigurationProvider.Get<BotSecrets>().Credentials.ToJson())
                .CreateScoped(scopes);

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            return service.Spreadsheets;
        }
    }
        
    protected SpreadsheetsResource.ValuesResource.GetRequest GetRequest(string range)
        => GoogleSheetsRequestBuilder.InitializeSpreadSheetId(SpreadSheetId, Resource)
            .SetupRange(range)
            .ToGetRequest();
        
    protected SpreadsheetsResource.ValuesResource.AppendRequest AppendRequest(string range, IEnumerable<IList<object>> values)
        => GoogleSheetsRequestBuilder.InitializeSpreadSheetId(SpreadSheetId, Resource)
            .SetupRange(range)
            .AddValues(values)
            .ToAppendRequest();
        
    protected SpreadsheetsResource.ValuesResource.AppendRequest AppendRequest(string range, IList<object> values)
        => GoogleSheetsRequestBuilder.InitializeSpreadSheetId(SpreadSheetId, Resource)
            .SetupRange(range)
            .AddValues(values)
            .ToAppendRequest();

    protected SpreadsheetsResource.ValuesResource.UpdateRequest UpdateRequest(string range, IList<object> values)
        => GoogleSheetsRequestBuilder.InitializeSpreadSheetId(SpreadSheetId, Resource)
            .SetupRange(range)
            .AddValues(values)
            .ToUpdateRequest();
}