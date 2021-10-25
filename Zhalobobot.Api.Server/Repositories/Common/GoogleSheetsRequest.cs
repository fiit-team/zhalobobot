using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Zhalobobot.Api.Server.Repositories.Common
{
    public class GoogleSheetsRequest
    {
        private string SpreadSheetId { get; }
        private List<IList<object>> Values { get; }
        private SpreadsheetsResource Resource { get; }
        private string? Range { get; set; }

        public GoogleSheetsRequest(string spreadSheetId, SpreadsheetsResource resource)
        {
            SpreadSheetId = spreadSheetId;
            Values = new List<IList<object>>();
            Resource = resource;
        }

        public GoogleSheetsRequest AddValues(IEnumerable<IList<object>> values)
        {
            Values.AddRange(values);
            return this;
        }

        public GoogleSheetsRequest AddValues(IList<object> values)
        {
            Values.Add(values);
            return this;
        }

        public GoogleSheetsRequest SetupRange(string range)
        {
            Range = range;
            return this;
        }
        
        public SpreadsheetsResource.ValuesResource.AppendRequest ToAppendRequest()
        {
            var body = new ValueRange { Values = Values };

            var request = Resource.Values.Append(body, SpreadSheetId, Range);
            
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            return request;
        }
        
        public SpreadsheetsResource.ValuesResource.AppendRequest ToAppendRawRequest()
        {
            var body = new ValueRange { Values = Values };

            var request = Resource.Values.Append(body, SpreadSheetId, Range);
            
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

            return request;
        }
        
        public SpreadsheetsResource.ValuesResource.GetRequest ToGetRequest()
            => Resource.Values.Get(SpreadSheetId, Range);
    }

}