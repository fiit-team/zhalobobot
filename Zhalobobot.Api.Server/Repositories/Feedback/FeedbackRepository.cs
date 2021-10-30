using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public class FeedbackRepository : GoogleSheetsRepositoryBase, IFeedbackRepository
    {
        private IConfiguration Configuration { get; }

        public FeedbackRepository(
            IConfiguration configuration)
        : base(configuration, configuration["FeedbackSpreadSheetId"])
        {
            Configuration = configuration;
        }

        public async Task AddFeedback(Zhalobobot.Common.Models.Feedback.Feedback feedback)
        {
            var objectList = new List<object>
            {
                EkbTime().ToString(CultureInfo.InvariantCulture),
                feedback.Type.GetString(),
                feedback.Subject.Name,
                feedback.Message ?? string.Empty,
                feedback.Student!.InGroupA ? "A" : "B",
                feedback.Student!.TelegramId,
                feedback.Student!.Name?.ToString() ?? string.Empty,
                feedback.Student!.GroupNumber?.ToString() ?? string.Empty,
                feedback.Student!.SubgroupNumber?.ToString() ?? string.Empty,
                feedback.Student!.AdmissionYear?.ToString() ?? string.Empty
            };
            
            await StartGoogleSheetsRequest()
                .AddValues(objectList)
                .SetupRange("Feedback!A:J")
                .ToAppendRequest()
                .ExecuteAsync();
        }
    }
}