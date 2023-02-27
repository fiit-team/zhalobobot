using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Bot.Api.Models;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Bot.Api.Repositories.Common;

public abstract class GoogleSheetsRepositoryBase
{
    private SpreadsheetsResource Resource { get; }
    private string SpreadSheetId { get; }

    protected GoogleSheetsRepositoryBase(IConfiguration configuration, string spreadSheetId)
    {
        SpreadSheetId = spreadSheetId;
        Resource = GetSpreadsheetsResource();

        SpreadsheetsResource GetSpreadsheetsResource()
        {
            var scopes = new[] { SheetsService.Scope.Spreadsheets };

            var credential = GoogleCredential
                .FromJson(configuration.GetSection("CREDENTIALS").Get<Credentials>().ToJson())
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