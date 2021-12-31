using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Models;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Api.Server.Repositories.Common
{
    public abstract class GoogleSheetsRepositoryBase
    {
        private SpreadsheetsResource Resource { get; }
        private string SpreadSheetId { get; }
        private IConfiguration Configuration { get; }

        protected GoogleSheetsRepositoryBase(IConfiguration configuration, string spreadSheetId)
        {
            SpreadSheetId = spreadSheetId;
            Resource = GetSpreadsheetsResource();
            Configuration = configuration;

            SpreadsheetsResource GetSpreadsheetsResource()
            {
                var scopes = new[] { SheetsService.Scope.Spreadsheets };

                GoogleCredential credential = GoogleCredential
                    .FromJson(configuration.GetSection("CREDENTIALS").Get<Credentials>().ToJson())
                    .CreateScoped(scopes);

                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });

                return service.Spreadsheets;
            }
        }

        protected GoogleSheetsRequest StartGoogleSheetsRequest()
            => GoogleSheetsRequestBuilder.InitializeSpreadSheetId(SpreadSheetId, Resource);

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
    }
}