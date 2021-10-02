using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Repositories
{
    public class GoogleSheetsRepository : IFeedbackRepository
    {
        private SpreadsheetsResource Resource { get; }
        private Settings Settings { get; }
        private ILogger<IFeedbackRepository> Logger { get; }

        public GoogleSheetsRepository(
            SpreadsheetsResource resource,
            Settings settings,
            ILogger<IFeedbackRepository> logger)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddFeedbackInfoAsync(FeedbackInfo feedbackInfo)
        {
            var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            var ekbTime = DateTime.UtcNow + moscowTimeZone.BaseUtcOffset + TimeSpan.FromHours(2);

            var objectList = new List<object>() {
                ekbTime.ToString(),
                feedbackInfo.Subject ?? "Общая",
                feedbackInfo.Message,                 
            };

            var valueRange = new ValueRange() { Values = new List<IList<object>> { objectList } };

            var range = "Feedback!A:C";

            var appendRequest = this.Resource.Values.Append(valueRange, this.Settings.SpreadSheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var _ = await appendRequest.ExecuteAsync();
        }
    }
}
