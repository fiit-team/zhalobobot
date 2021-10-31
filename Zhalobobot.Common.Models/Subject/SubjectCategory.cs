using System.ComponentModel;

namespace Zhalobobot.Common.Models.Subject
{
    public enum SubjectCategory
    {
        [Description("Математика")]
        Math = 0,
        [Description("Программирование")]
        Programming = 1,
        [Description("Онлайн курсы")]
        OnlineCourse = 2,
        [Description("Другое")]
        Another = 3
    }
}