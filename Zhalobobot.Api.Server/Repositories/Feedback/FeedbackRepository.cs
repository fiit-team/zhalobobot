using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public class FeedbackRepository : GoogleSheetsRepositoryBase, IFeedbackRepository
    {
        private ILogger<IFeedbackRepository> Logger { get; }
        private IConfiguration Configuration { get; }

        public FeedbackRepository(
            ILogger<IFeedbackRepository> logger,
            IConfiguration configuration)
        : base(configuration, configuration["FeedbackSpreadSheetId"], "FEEDBACK_SPREADSHEET_CREDENTIALS")
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Configuration = configuration;
        }

        public async Task AddFeedback(Zhalobobot.Common.Models.Feedback.Feedback feedback)
        {
            var objectList = new List<object>
            {
                EkbTime.ToString(CultureInfo.InvariantCulture),
                feedback.Type.GetString(),
                feedback.Subject.Name,
                feedback.Message ?? ""
            };
            
            await StartGoogleSheetsRequest()
                .AddValues(objectList)
                .SetupRange("Feedback!A:D")
                .ToAppendRequest()
                .ExecuteAsync();
        }
    }
}