namespace Zhalobobot.Bot.Models
{
    public static class CallbackDataPrefix
    {
        public const string Rating = "rating";
        public const string SubjectCategory = "subj_cat";
        public const string Subject = "subj";
        public const string AddCourse = "addCourse";
        public const string AddCourseAndGroup = "addCourseAndGroup";
        public const string AddCourseAndGroupAndSubgroup = "addCourseAndGroupAndSubgroup";
        public const string Feedback = "feedback";
        public const string ChooseScheduleRange = "chooseScheduleRange";
        public const string PaginationButton = "PaginationButton";
        public const string PaginationItemButton = "PaginationItemButton";
        public const string InternalDoNothing = "InternalDoNothing";
        public const string SubmitSpecialCourses = "SubmitSpecialCourses";
        public const string NotVisitedPair = "NotVisitedPair";

        public static string Nothing => $"{InternalDoNothing}{Strings.Separator}_";
    }
}
