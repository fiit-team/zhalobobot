using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Zhalobobot.Common.Models.Feedback;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Api.Server.Repositories
{
    public class GoogleSheetsRepository : IGoogleSheetsRepository
    {
        private SpreadsheetsResource Resource { get; }
        private Settings Settings { get; }
        private ILogger<IGoogleSheetsRepository> Logger { get; }

        public GoogleSheetsRepository(
            SpreadsheetsResource resource,
            Settings settings,
            ILogger<IGoogleSheetsRepository> logger)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddFeedbackInfo(Feedback feedback)
        {
            var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            var ekbTime = DateTime.UtcNow + moscowTimeZone.BaseUtcOffset + TimeSpan.FromHours(2);

            var objectList = new List<object>
            {
                ekbTime.ToString(CultureInfo.InvariantCulture),
                feedback.Type.GetString(),
                feedback.Subject.Name,
                feedback.Message,
            };

            var valueRange = new ValueRange { Values = new List<IList<object>> { objectList } };

            var range = "Feedback!A:D";

            var appendRequest = Resource.Values.Append(valueRange, Settings.SpreadSheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var _ = await appendRequest.ExecuteAsync();
        }
    }
}