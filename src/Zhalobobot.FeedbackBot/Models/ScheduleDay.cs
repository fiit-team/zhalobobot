using System.ComponentModel;

namespace Zhalobobot.Bot.Models
{
    public enum ScheduleDay
    {
        [Description("Понедельник")]
        Monday = 1,
        [Description("Вторник")]
        Tuesday = 2,
        [Description("Среда")]
        Wednesday = 3,
        [Description("Четверг")]
        Thursday = 4,
        [Description("Пятница")]
        Friday = 5,
        [Description("Суббота")]
        Saturday = 6,
        FullWeek = 7,
        UntilWeekEnd = 8,
        NextMonday = 9,
        NextWeek = 10
    }
}