using Google.Apis.Sheets.v4;

namespace Zhalobobot.Api.Server.Repositories.Common
{
    public static class GoogleSheetsRequestBuilder
    {
        public static GoogleSheetsRequest InitializeSpreadSheetId(string spreadSheetId, SpreadsheetsResource resource)
            => new(spreadSheetId, resource);
    }
}