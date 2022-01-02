using System.ComponentModel;

namespace Zhalobobot.Common.Models.Subject
{
    public enum SubjectCategory
    {
        [Description("Математика")]
        Math = 1,
        [Description("Программирование")]
        Programming = 2,
        [Description("Онлайн курсы")]
        OnlineCourse = 3,
        [Description("Другое")]
        Another = 4
    }
}