using System;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Helpers.Helpers
{
    public static class ParsingHelper
    {
        public static Name ParseName(object lastName, object firstName, object middleName)
            => new(lastName as string ?? "", firstName as string ?? "", middleName as string);

        public static int? ParseNullableInt(object value)
            => int.TryParse(value as string, out var result) ? result : null;

        public static int ParseInt(object value)
            => int.TryParse(value as string, out var result) ? result : throw new Exception();

        public static bool ParseBool(object value)
            => value as string == "TRUE";

        public static SubjectCategory ParseSubjectCategory(object value)
        {
            var str = value as string ?? throw new ArgumentException("Subject is empty");

            return str switch
            {
                "Математика" => SubjectCategory.Math,
                "Программирование" => SubjectCategory.Programming,
                "Онлайн курсы" => SubjectCategory.OnlineCourse,
                "Другое" => SubjectCategory.Another,
                _ => throw new NotSupportedException(str)
            };
        }
        
        public static DayOfWeek ParseDay(object value)
        {
            return (value as string) switch
            {
                "Понедельник" => DayOfWeek.Monday,
                "Вторник" => DayOfWeek.Tuesday,
                "Среда" => DayOfWeek.Wednesday,
                "Четверг" => DayOfWeek.Thursday,
                "Пятница" => DayOfWeek.Friday,
                "Суббота" => DayOfWeek.Saturday,
                _ => throw new NotImplementedException()
            };
        }
    }
}