using Zhalobobot.Bot.Api.Models;

namespace Zhalobobot.Bot.Settings;

public class BotSecrets
{ 
    public bool IsFirstYearWeekOdd { get; set; } = false;
    public BotConfiguration BotConfiguration { get; set; } = null!;
    public string StudentIdToIgnoreWhenDesign { get; set; } = null!;
    public Settings Settings { get; set; } = null!;
    public QuartzSettings Quartz { get; set; } = null!;
    public string FeedbackSpreadSheetId { get; set; } = null!;
    public string ScheduleSpreadSheetId { get; set; } = null!;
    public string FiitStudentsDataSpreadSheetId { get; set; } = null!;
    public string StudentsDataRange { get; set; } = null!;
    public string StudentsRange { get; set; } = null!;
    public string SubjectsRange { get; set; } = null!;
    public string FeedbackRange { get; set; } = null!;
    public string ScheduleRange { get; set; } = null!;
    public string RepliesRange { get; set; } = null!;
    public string FeedbackChatDataRange { get; set; } = null!;
    public string DayWithoutPairsRange { get; set; } = null!;
    public Credentials Credentials { get; set; } = null!;
}