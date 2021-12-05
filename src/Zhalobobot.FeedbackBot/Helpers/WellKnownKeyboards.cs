using System;
using System.Linq;
using EnumsNET;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Helpers
{
    public static class WellKnownKeyboards
    {
        public static ReplyKeyboardMarkup DefaultKeyboard { get; } = new(
            new[]
            {
                new KeyboardButton[] { Buttons.Subjects },
                new KeyboardButton[] { Buttons.GeneralFeedback },
                new KeyboardButton[] { Buttons.Alarm }
            })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup SubmitKeyboard { get; } = new(
            new KeyboardButton[]
            {
                Buttons.Submit,
                Buttons.MainMenu
            })
        {
            ResizeKeyboard = true
        };

        public static InlineKeyboardMarkup AddCourseKeyboard { get; } =
            new(Enum.GetValues<Course>()
                .Select(course => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        course.AsString(EnumFormat.Description),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.AddCourse, course))
                }));

        public static InlineKeyboardMarkup AddCourseAndGroupKeyboard(Course course) =>
            new (Enum.GetValues<Group>()
                .Take(course == Course.Third ? 2 : 4)
                .Select(group => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        group.AsString(EnumFormat.Description),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.AddCourseAndGroup, course, group))
                }));
        
        public static InlineKeyboardMarkup AddCourseAndGroupAndSubgroupKeyboard(Course course, Group group) =>
            new (Enum.GetValues<Subgroup>()
                .Select(subgroup => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        subgroup.AsString(EnumFormat.Description),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.AddCourseAndGroupAndSubgroup, course, group, subgroup))
                }));

        public static InlineKeyboardMarkup RatingKeyboard { get; } =
            new (Enumerable.Range(1, 5)
                .Select(star => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", Enumerable.Repeat(Emoji.Star, star)),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.Rating, star))
                }));
        
        public static InlineKeyboardMarkup SendFeedbackKeyboard(string subjectName) => 
            new []
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        "Оставить обратную связь",
                        Utils.Join(Strings.Separator, CallbackDataPrefix.Feedback, subjectName))                
                }
            };

        public static InlineKeyboardMarkup GetSubjectCategoryKeyboard =>  
            new (Enum.GetValues<SubjectCategory>()
                    .Select(category => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            category.AsString(EnumFormat.Description),
                            Utils.Join(Strings.Separator, CallbackDataPrefix.SubjectCategory, category))
                    }));
    }
}