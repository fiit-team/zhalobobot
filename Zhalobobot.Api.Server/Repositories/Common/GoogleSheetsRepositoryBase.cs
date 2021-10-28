using System;
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
        protected Func<DateTime> EkbTime { get; } 
        private SpreadsheetsResource Resource { get; }
        private string SpreadSheetId { get; }
        private IConfiguration Configuration { get; }

        protected GoogleSheetsRepositoryBase(IConfiguration configuration, string spreadSheetId)
        {
            SpreadSheetId = spreadSheetId;
            EkbTime = GetEkbTime;
            Resource = GetSpreadsheetsResource();
            Configuration = configuration;

            DateTime GetEkbTime()
            {
                var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            
                return DateTime.UtcNow + moscowTimeZone.BaseUtcOffset + TimeSpan.FromHours(2);
            }
            
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
    }
}