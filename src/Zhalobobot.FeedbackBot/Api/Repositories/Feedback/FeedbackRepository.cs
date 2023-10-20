using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Api.Repositories.Common;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Bot.Api.Repositories.Feedback;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public class FeedbackRepository : GoogleSheetsRepositoryBase, IFeedbackRepository
{
    private ILogger<FeedbackRepository> Logger { get; }
    private string FeedbackRange { get; }

    public FeedbackRepository(IVostokHostingEnvironment environment, ILogger<FeedbackRepository> logger)
        : base(environment, environment.SecretConfigurationProvider.Get<BotSecrets>().FeedbackSpreadSheetId)
    {
        Logger = logger;
        FeedbackRange = environment.SecretConfigurationProvider.Get<BotSecrets>().FeedbackRange;
    }

    public async Task Add(Zhalobobot.Common.Models.Feedback.Feedback feedback)
    {
        var objectList = new List<object>
        {
            DateHelper.EkbTime.ToString(CultureInfo.InvariantCulture),
            feedback.Type.GetString(),
            feedback.Subject?.Name ?? string.Empty,
            feedback.SubjectSurvey?.Rating.ToString() ?? string.Empty,
            feedback.SubjectSurvey is null ? string.Empty : string.Join("; ", feedback.SubjectSurvey.LikedPoints),
            feedback.SubjectSurvey is null ? string.Empty : string.Join("; ", feedback.SubjectSurvey.UnlikedPoints),
            feedback.Message ?? string.Empty,
            feedback.Student.Id,
            feedback.Student.Username ?? string.Empty,
            feedback.Student.Name?.ToString() ?? string.Empty,
            feedback.Student.Group,
            feedback.Student.Subgroup,
            feedback.Student.Course,
            feedback.MessageId
        };
            
        await AppendRequest(FeedbackRange, objectList).ExecuteAsync();
    }
}